use std::collections::{BTreeMap, BTreeSet};
use std::fs;
use std::path::{Path, PathBuf};
use syn::{
    Expr, ExprLit, FnArg, GenericArgument, Item, ItemTrait, Lit, Meta, Pat, PathArguments,
    ReturnType, TraitItem, Type,
};

struct Param {
    name: String,
    ts_type: String,
    rust_type: String,
    docs: Vec<String>,
    /// For a `tauri::ipc::Channel<T>` param: `Some((ts_inner, rust_inner))`.
    /// When set, `ts_type`/`rust_type` are unused.
    channel_inner: Option<(String, String)>,
}

struct Method {
    rust_name: String,
    ts_name: String,
    command: String,
    params: Vec<Param>,
    ret_ts: String,
    ret_rust: String,
    docs: Vec<String>,
}

struct Api {
    namespace: String,
    module: String,
    class_name: String,
    docs: Vec<String>,
    methods: Vec<Method>,
}

/// One backend→frontend event (one method of a `#[vla_events]` trait).
struct Event {
    rust_name: String,
    ts_name: String,
    /// Wire event name, `format!("{namespace}_{rust_name}")` (same scheme as commands).
    wire: String,
    payload_ts: String,
    payload_rust: String,
    docs: Vec<String>,
}

/// A `#[vla_events(namespace = "...")]` trait — a pure manifest, never implemented.
struct EventApi {
    namespace: String,
    module: String,
    class_name: String,
    docs: Vec<String>,
    events: Vec<Event>,
}

/// A `#[vla_service]` free fn: a background task started once on app startup.
/// Resolved as `crate::app::<module>::<name>(app.clone())`.
struct Service {
    module: String,
    name: String,
}

/// Regenerates schema artifacts. `root` is the repo root (paths below are
/// resolved against it, so this works from any cwd — `cargo run` from the repo
/// root or a `build.rs` whose cwd is the crate dir). With `rust_only`, only
/// `_generated.rs` is rewritten (no TS, no ts-rs bindings) — that's the cheap
/// step `build.rs` needs so the backend always compiles against a fresh file.
pub fn generate(root: &Path, rust_only: bool) {
    let src_dir = root.join("app/backend/src");
    // ts-rs writes per-type intermediates here (TS_RS_EXPORT_DIR, set by the
    // justfile). It lives under target/ so no codegen artifact lands in /backend.
    let bindings_dir = std::env::var_os("TS_RS_EXPORT_DIR")
        .map(PathBuf::from)
        .unwrap_or_else(|| root.join("target/schema-bindings"));
    let per_type_dir = bindings_dir.join("_per_type");
    // Final namespace files are written straight into the frontend.
    let client_dir = root.join("app/frontend/lib/schema");
    let tauri_generated = root.join("app/backend/src/_generated.rs");

    let mut type_to_module: BTreeMap<String, String> = BTreeMap::new();
    let mut module_types: BTreeMap<String, Vec<String>> = BTreeMap::new();
    let mut apis: Vec<Api> = Vec::new();
    let mut event_apis: Vec<EventApi> = Vec::new();
    let mut services: Vec<Service> = Vec::new();
    let mut imports: BTreeSet<String> = BTreeSet::new();

    let mut rs_files: Vec<PathBuf> = Vec::new();
    collect_rs_files(&src_dir, &mut rs_files);
    rs_files.sort();
    for path in &rs_files {
        let module = path.file_stem().unwrap().to_string_lossy().into_owned();
        // Skip module-tree plumbing and generated files; only files that
        // actually contain #[vla_type]/#[vla_api] items become TS modules.
        if matches!(module.as_str(), "lib" | "main" | "mod" | "_generated") {
            continue;
        }
        let src = fs::read_to_string(path).expect("read source file");
        let Ok(file) = syn::parse_file(&src) else {
            continue;
        };
        for item in &file.items {
            match item {
                Item::Struct(s) if has_attr(&s.attrs, "vla_type") => {
                    let name = s.ident.to_string();
                    type_to_module.insert(name.clone(), module.clone());
                    module_types.entry(module.clone()).or_default().push(name);
                }
                Item::Enum(e) if has_attr(&e.attrs, "vla_type") => {
                    let name = e.ident.to_string();
                    type_to_module.insert(name.clone(), module.clone());
                    module_types.entry(module.clone()).or_default().push(name);
                }
                Item::Fn(f) if has_attr(&f.attrs, "vla_service") => {
                    services.push(Service {
                        module: module.clone(),
                        name: f.sig.ident.to_string(),
                    });
                }
                Item::Trait(t) => {
                    if let Some(namespace) = extract_attr_namespace(t, "vla_api") {
                        apis.push(parse_trait(t, namespace, module.clone(), &mut imports));
                    } else if let Some(namespace) = extract_attr_namespace(t, "vla_events") {
                        event_apis.push(parse_events_trait(
                            t,
                            namespace,
                            module.clone(),
                            &mut imports,
                        ));
                    }
                }
                _ => {}
            }
        }
    }

    apis.sort_by(|a, b| a.namespace.cmp(&b.namespace));
    event_apis.sort_by(|a, b| a.namespace.cmp(&b.namespace));
    services.sort_by(|a, b| (&a.module, &a.name).cmp(&(&b.module, &b.name)));
    write_tauri_handler(&tauri_generated, &apis, &event_apis, &services);

    if rust_only {
        return;
    }

    sweep_stale_ts(&bindings_dir);
    sweep_stale_ts(&client_dir);

    let all_modules: BTreeSet<String> = module_types
        .keys()
        .cloned()
        .chain(apis.iter().map(|a| a.module.clone()))
        .chain(event_apis.iter().map(|a| a.module.clone()))
        .collect();
    for module in &all_modules {
        let types = module_types.get(module).cloned().unwrap_or_default();
        let api = apis.iter().find(|a| &a.module == module);
        let event_api = event_apis.iter().find(|a| &a.module == module);
        write_namespace_file(
            &client_dir,
            module,
            &types,
            api,
            event_api,
            &per_type_dir,
            &type_to_module,
        );
    }

    write_index(
        &client_dir.join("index.ts"),
        &apis,
        &event_apis,
        &type_to_module,
    );
}

/// Recursively collects every `.rs` file under `dir`.
fn collect_rs_files(dir: &Path, out: &mut Vec<PathBuf>) {
    let Ok(entries) = fs::read_dir(dir) else {
        return;
    };
    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_dir() {
            collect_rs_files(&path, out);
        } else if path.extension().and_then(|e| e.to_str()) == Some("rs") {
            out.push(path);
        }
    }
}

/// Strips `[]` / ` | null` wrappers to get the underlying type name, so a
/// `Vec<Brick>` return (`Brick[]`) still resolves its cross-module import.
fn base_ts_name(ts: &str) -> &str {
    let mut s = ts.trim();
    loop {
        if let Some(x) = s.strip_suffix(" | null") {
            s = x.trim();
        } else if let Some(x) = s.strip_suffix("[]") {
            s = x.trim();
        } else {
            break;
        }
    }
    s
}

fn sweep_stale_ts(dir: &Path) {
    let Ok(entries) = fs::read_dir(dir) else {
        return;
    };
    for entry in entries.flatten() {
        let path = entry.path();
        if !path.is_file() {
            continue;
        }
        if path.extension().and_then(|e| e.to_str()) != Some("ts") {
            continue;
        }
        let name = path.file_name().and_then(|n| n.to_str()).unwrap_or("");
        if name == "rpc.ts" {
            continue;
        }
        let _ = fs::remove_file(&path);
    }
}

fn write_tauri_handler(
    out_path: &Path,
    apis: &[Api],
    event_apis: &[EventApi],
    services: &[Service],
) {
    let mut out = String::new();
    out.push_str("// Generated by schema-codegen. Do not edit.\n\n");
    out.push_str("#![allow(clippy::needless_lifetimes)]\n\n");
    out.push_str("// Note: command return types and event payload types are resolved\n");
    out.push_str("// via `use crate::app::<module>::*;` below. Types that live outside\n");
    out.push_str("// `app/` must be `pub use`-re-exported from their app module\n");
    out.push_str("// (see `pub use crate::bricks::types::Brick;` in app/bricks.rs).\n");
    out.push_str("use crate::app::App;\n\n");

    let mut modules_used: BTreeSet<String> = BTreeSet::new();
    for api in apis {
        modules_used.insert(api.module.clone());
    }
    for ev in event_apis {
        modules_used.insert(ev.module.clone());
    }
    modules_used.insert("error".to_string());
    for module in &modules_used {
        out.push_str(&format!("use crate::app::{module}::*;\n"));
    }
    if !modules_used.is_empty() {
        out.push('\n');
    }

    for api in apis {
        for m in &api.methods {
            let param_decls: Vec<String> = m
                .params
                .iter()
                .map(|p| match &p.channel_inner {
                    Some((_, rust_inner)) => {
                        format!("{}: tauri::ipc::Channel<{rust_inner}>", p.name)
                    }
                    None => format!("{}: {}", p.name, p.rust_type),
                })
                .collect();
            let param_names: Vec<String> = m.params.iter().map(|p| p.name.clone()).collect();
            let mut all_params = vec!["state: tauri::State<'_, App>".to_string()];
            all_params.extend(param_decls);

            out.push_str("#[tauri::command(rename_all = \"snake_case\")]\n");
            out.push_str(&format!(
                "pub async fn {cmd}({params}) -> {ret} {{\n",
                cmd = m.command,
                params = all_params.join(", "),
                ret = m.ret_rust,
            ));
            out.push_str(&format!(
                "    <App as {trait_name}>::{method}(&state{call_args}).await\n",
                trait_name = api.class_name,
                method = m.rust_name,
                call_args = if param_names.is_empty() {
                    String::new()
                } else {
                    format!(", {}", param_names.join(", "))
                },
            ));
            out.push_str("}\n\n");
        }
    }

    write_emit_impls(&mut out, event_apis);

    out.push_str(
        "/// Starts every `#[vla_service]` background task. Called once from\n\
         /// the Tauri setup hook, after `App.handle` is set.\n",
    );
    out.push_str("#[allow(unused_variables)]\n");
    out.push_str("pub fn start_services(app: &App) {\n");
    for s in services {
        out.push_str(&format!(
            "    crate::app::{module}::{name}(app);\n",
            module = s.module,
            name = s.name,
        ));
    }
    out.push_str("}\n\n");

    out.push_str("pub fn invoke_handler<R: tauri::Runtime>(\n");
    out.push_str(") -> impl Fn(tauri::ipc::Invoke<R>) -> bool + Send + Sync + 'static {\n");
    if apis.iter().all(|a| a.methods.is_empty()) {
        out.push_str("    tauri::generate_handler![]\n");
    } else {
        out.push_str("    tauri::generate_handler![\n");
        for api in apis {
            for m in &api.methods {
                out.push_str(&format!("        {},\n", m.command));
            }
        }
        out.push_str("    ]\n");
    }
    out.push_str("}\n");

    if let Some(parent) = out_path.parent() {
        fs::create_dir_all(parent).expect("create tauri src dir");
    }
    if let Ok(existing) = fs::read_to_string(out_path) {
        if existing == out {
            return;
        }
    }
    fs::write(out_path, out).expect("write tauri _generated.rs");
}

/// Emits one `{Ns}Emitter` struct per `#[vla_events]` namespace plus an
/// aggregate `Events` struct (exposed on `App` as `app.events.<namespace>`),
/// each backed by the `AppHandle` stashed in `App` (no-op until the setup hook
/// runs). Grouping under `events` keeps namespaces collision-free, so no
/// cross-namespace dedup guard is needed (two methods of the same name in one
/// trait is itself a rustc error).
fn write_emit_impls(out: &mut String, event_apis: &[EventApi]) {
    for ev in event_apis {
        let pascal = capitalize(&snake_to_camel(&ev.namespace));
        out.push_str("#[derive(Clone)]\n");
        out.push_str(&format!(
            "pub struct {pascal}Emitter {{ handle: crate::shared::Shared<Option<tauri::AppHandle>> }}\n\n"
        ));
        out.push_str(&format!("impl {pascal}Emitter {{\n"));
        for e in &ev.events {
            let payload = last_path_segment(&e.payload_rust);
            out.push_str(&format!(
                "    /// Emits the `{wire}` event to all frontends.\n",
                wire = e.wire
            ));
            out.push_str("    #[allow(dead_code)]\n");
            out.push_str(&format!(
                "    pub fn emit_{name}(&self, payload: &{payload}) -> tauri::Result<()> {{\n",
                name = e.rust_name,
            ));
            out.push_str("        match self.handle.get() {\n");
            out.push_str(&format!(
                "            Some(h) => ::tauri::Emitter::emit(&h, \"{wire}\", payload.clone()),\n",
                wire = e.wire,
            ));
            out.push_str("            None => Ok(()),\n");
            out.push_str("        }\n");
            out.push_str("    }\n");
        }
        out.push_str("}\n\n");
    }

    // Always emitted (even with zero event namespaces) so `app/mod.rs`'s
    // `pub events: crate::_generated::Events` field always resolves.
    out.push_str("#[derive(Clone)]\n");
    out.push_str("pub struct Events {\n");
    for ev in event_apis {
        let pascal = capitalize(&snake_to_camel(&ev.namespace));
        out.push_str(&format!(
            "    pub {ns}: {pascal}Emitter,\n",
            ns = ev.namespace
        ));
    }
    out.push_str("}\n\n");

    let handle_param = if event_apis.is_empty() {
        "_handle"
    } else {
        "handle"
    };
    out.push_str("impl Events {\n");
    out.push_str(&format!(
        "    pub fn new({handle_param}: crate::shared::Shared<Option<tauri::AppHandle>>) -> Self {{\n"
    ));
    out.push_str("        Self {\n");
    for ev in event_apis {
        let pascal = capitalize(&snake_to_camel(&ev.namespace));
        out.push_str(&format!(
            "            {ns}: {pascal}Emitter {{ handle: handle.clone() }},\n",
            ns = ev.namespace
        ));
    }
    out.push_str("        }\n");
    out.push_str("    }\n");
    out.push_str("}\n\n");
}

/// Last `::` segment of a Rust type path (`crate::a::Foo` -> `Foo`), so the
/// `use crate::api::<module>::*;` glob in `_generated.rs` resolves it. Generic
/// types (`Vec<T>`, `Option<T>`) are std and passed through verbatim.
fn last_path_segment(rust_ty: &str) -> String {
    if rust_ty.contains('<') {
        return rust_ty.to_string();
    }
    rust_ty.rsplit("::").next().unwrap_or(rust_ty).to_string()
}

fn write_namespace_file(
    client_dir: &Path,
    module: &str,
    types: &[String],
    api: Option<&Api>,
    event_api: Option<&EventApi>,
    per_type_dir: &Path,
    type_to_module: &BTreeMap<String, String>,
) {
    let mut cross_imports: BTreeMap<String, BTreeSet<String>> = BTreeMap::new();
    let mut type_bodies: Vec<String> = Vec::new();

    for ty in types {
        let file = per_type_dir.join(format!("{ty}.ts"));
        let raw = fs::read_to_string(&file).unwrap_or_else(|_| {
            panic!("missing per-type binding {}", file.display());
        });

        for line in raw.lines() {
            let trimmed = line.trim();
            if let Some(rest) = trimmed.strip_prefix("import type { ") {
                let Some(name_end) = rest.find(" }") else {
                    continue;
                };
                let names: Vec<String> = rest[..name_end]
                    .split(',')
                    .map(|s| s.trim().to_string())
                    .filter(|s| !s.is_empty())
                    .collect();
                for name in names {
                    let Some(other_module) = type_to_module.get(&name) else {
                        panic!("imported type {name} not found in any module");
                    };
                    if other_module != module {
                        cross_imports
                            .entry(other_module.clone())
                            .or_default()
                            .insert(name);
                    }
                }
                continue;
            }
            if trimmed.starts_with("import ") || trimmed.starts_with("// This file was generated") {
                continue;
            }
            type_bodies.push(line.to_string());
        }
        type_bodies.push(String::new());
    }

    let add_cross = |ts: &str, cross: &mut BTreeMap<String, BTreeSet<String>>| {
        let base = base_ts_name(ts);
        if let Some(other_module) = type_to_module.get(base) {
            if other_module != module {
                cross
                    .entry(other_module.clone())
                    .or_default()
                    .insert(base.to_string());
            }
        }
    };
    if let Some(api) = api {
        for m in &api.methods {
            for p in &m.params {
                let ts = match &p.channel_inner {
                    Some((ts_inner, _)) => ts_inner.as_str(),
                    None => p.ts_type.as_str(),
                };
                add_cross(ts, &mut cross_imports);
            }
            add_cross(&m.ret_ts, &mut cross_imports);
        }
    }
    if let Some(ev) = event_api {
        for e in &ev.events {
            add_cross(&e.payload_ts, &mut cross_imports);
        }
    }

    let mut out = String::new();
    out.push_str("// Generated by schema-codegen. Do not edit.\n\n");
    let needs_invoke = api.is_some();
    let needs_channel = api.is_some_and(|a| {
        a.methods
            .iter()
            .any(|m| m.params.iter().any(|p| p.channel_inner.is_some()))
    });
    let needs_listen = event_api.is_some();
    let mut rpc_named: Vec<&str> = Vec::new();
    if needs_invoke {
        rpc_named.push("invoke");
    }
    if needs_channel {
        rpc_named.push("Channel");
    }
    if needs_listen {
        rpc_named.push("listen");
    }
    if !rpc_named.is_empty() {
        out.push_str(&format!(
            "import {{ {} }} from \"./rpc\";\n",
            rpc_named.join(", ")
        ));
    }
    if needs_listen {
        out.push_str("import type { UnlistenFn } from \"./rpc\";\n");
    }
    for (m, names) in &cross_imports {
        let list = names.iter().cloned().collect::<Vec<_>>().join(", ");
        out.push_str(&format!("import type {{ {list} }} from \"./{m}\";\n"));
    }
    out.push('\n');

    let mut bodies = String::new();
    for line in type_bodies {
        bodies.push_str(&line);
        bodies.push('\n');
    }
    out.push_str(&format_ts(&bodies));
    while out.ends_with("\n\n\n") {
        out.pop();
    }

    if let Some(api) = api {
        emit_sub_class(&mut out, api);
    }
    if let Some(ev) = event_api {
        emit_events_class(&mut out, ev);
    }

    let dest = client_dir.join(format!("{module}.ts"));
    if let Some(parent) = dest.parent() {
        fs::create_dir_all(parent).expect("create client dir");
    }
    fs::write(&dest, out).expect("write namespace file");
}

fn write_index(
    out_path: &Path,
    apis: &[Api],
    event_apis: &[EventApi],
    type_to_module: &BTreeMap<String, String>,
) {
    let mut out = String::new();
    out.push_str("// Generated by schema-codegen. Do not edit.\n\n");

    // One import line per module, combining its command + events classes.
    let mut module_classes: BTreeMap<String, Vec<String>> = BTreeMap::new();
    for api in apis {
        module_classes
            .entry(api.module.clone())
            .or_default()
            .push(api.class_name.clone());
    }
    for ev in event_apis {
        module_classes
            .entry(ev.module.clone())
            .or_default()
            .push(ev.class_name.clone());
    }
    for (module, classes) in &module_classes {
        out.push_str(&format!(
            "import {{ {} }} from \"./{module}\";\n",
            classes.join(", ")
        ));
    }
    out.push('\n');

    let modules_with_types: BTreeSet<&String> = type_to_module.values().collect();
    for module in &modules_with_types {
        out.push_str(&format!("export type * from \"./{module}\";\n"));
    }
    if !modules_with_types.is_empty() {
        out.push('\n');
    }

    out.push_str("export class Api {\n");
    for api in apis {
        emit_jsdoc(&mut out, &api.docs, "\t");
        out.push_str(&format!(
            "\t{ns}: {cls};\n",
            ns = api.namespace,
            cls = api.class_name
        ));
    }
    for ev in event_apis {
        emit_jsdoc(&mut out, &ev.docs, "\t");
        out.push_str(&format!(
            "\t{ns}Events: {cls};\n",
            ns = ev.namespace,
            cls = ev.class_name
        ));
    }
    out.push_str("\n\tconstructor() {\n");
    for api in apis {
        out.push_str(&format!(
            "\t\tthis.{ns} = new {cls}();\n",
            ns = api.namespace,
            cls = api.class_name
        ));
    }
    for ev in event_apis {
        out.push_str(&format!(
            "\t\tthis.{ns}Events = new {cls}();\n",
            ns = ev.namespace,
            cls = ev.class_name
        ));
    }
    out.push_str("\t}\n");
    out.push_str("}\n\n");
    out.push_str("/** Shared singleton client. Import this instead of constructing `Api`. */\n");
    out.push_str("export const api = new Api();\n");

    if let Some(parent) = out_path.parent() {
        fs::create_dir_all(parent).expect("create client dir");
    }
    fs::write(out_path, out).expect("write index.ts");
}

fn parse_trait(
    t: &ItemTrait,
    namespace: String,
    module: String,
    imports: &mut BTreeSet<String>,
) -> Api {
    let mut methods = Vec::new();
    for item in &t.items {
        let TraitItem::Fn(method) = item else {
            continue;
        };
        let rust_name = method.sig.ident.to_string();
        let ts_name = snake_to_camel(&rust_name);
        let command = format!("{namespace}_{rust_name}");
        let docs = extract_docs(&method.attrs);

        let mut params = Vec::new();
        for arg in &method.sig.inputs {
            let FnArg::Typed(pat) = arg else { continue };
            let name = match &*pat.pat {
                Pat::Ident(p) => p.ident.to_string(),
                _ => panic!("unsupported param pattern in {}", method.sig.ident),
            };
            let docs = extract_docs(&pat.attrs);
            let (ts_type, rust_type, channel_inner) = if let Some(inner) = channel_inner_ty(&pat.ty)
            {
                (
                    String::new(),
                    String::new(),
                    Some((rust_type_to_ts(inner, imports), rust_type_to_string(inner))),
                )
            } else {
                (
                    rust_type_to_ts(&pat.ty, imports),
                    rust_type_to_string(&pat.ty),
                    None,
                )
            };
            params.push(Param {
                name,
                ts_type,
                rust_type,
                docs,
                channel_inner,
            });
        }

        let (ret_ts, ret_rust) = match &method.sig.output {
            ReturnType::Type(_, ty) => (
                extract_result_inner_ts(ty, imports),
                rust_type_to_string(ty),
            ),
            ReturnType::Default => ("void".into(), "()".into()),
        };

        methods.push(Method {
            rust_name,
            ts_name,
            command,
            params,
            ret_ts,
            ret_rust,
            docs,
        });
    }
    Api {
        namespace,
        module,
        class_name: t.ident.to_string(),
        docs: extract_docs(&t.attrs),
        methods,
    }
}

/// `Some(&T)` if `ty` is `Channel<T>` / `tauri::ipc::Channel<T>`.
fn channel_inner_ty(ty: &Type) -> Option<&Type> {
    if let Type::Path(p) = ty {
        let seg = p.path.segments.last()?;
        if seg.ident == "Channel" {
            return first_generic(&seg.arguments);
        }
    }
    None
}

fn parse_events_trait(
    t: &ItemTrait,
    namespace: String,
    module: String,
    imports: &mut BTreeSet<String>,
) -> EventApi {
    let mut events = Vec::new();
    for item in &t.items {
        let TraitItem::Fn(method) = item else {
            continue;
        };
        let rust_name = method.sig.ident.to_string();
        let docs = extract_docs(&method.attrs);
        // An event signature carries exactly one typed param: the payload.
        let mut payload_ts = "void".to_string();
        let mut payload_rust = "()".to_string();
        for arg in &method.sig.inputs {
            let FnArg::Typed(pat) = arg else { continue };
            payload_ts = rust_type_to_ts(&pat.ty, imports);
            payload_rust = rust_type_to_string(&pat.ty);
        }
        events.push(Event {
            ts_name: snake_to_camel(&rust_name),
            wire: format!("{namespace}_{rust_name}"),
            docs,
            rust_name,
            payload_ts,
            payload_rust,
        });
    }
    EventApi {
        namespace,
        module,
        class_name: t.ident.to_string(),
        docs: extract_docs(&t.attrs),
        events,
    }
}

fn emit_sub_class(out: &mut String, api: &Api) {
    out.push('\n');
    emit_jsdoc(out, &api.docs, "");
    out.push_str(&format!("export class {} {{\n", api.class_name));

    for m in &api.methods {
        let param_decl = m
            .params
            .iter()
            .map(|p| match &p.channel_inner {
                Some((ts_inner, _)) => format!("{}: (message: {ts_inner}) => void", p.name),
                None => format!("{}: {}", p.name, p.ts_type),
            })
            .collect::<Vec<_>>()
            .join(", ");

        // Channel params: build a Channel, wire its onmessage to the callback.
        let channel_setup: String = m
            .params
            .iter()
            .filter_map(|p| {
                p.channel_inner.as_ref().map(|(ts_inner, _)| {
                    format!(
                        "\t\tconst __{n} = new Channel<{ts_inner}>();\n\t\t__{n}.onmessage = {n};\n",
                        n = p.name,
                    )
                })
            })
            .collect();

        let invoke_args = if m.params.is_empty() {
            String::new()
        } else {
            let pairs = m
                .params
                .iter()
                .map(|p| {
                    if p.channel_inner.is_some() {
                        format!("{n}: __{n}", n = p.name)
                    } else {
                        p.name.clone()
                    }
                })
                .collect::<Vec<_>>()
                .join(", ");
            format!(", {{ {pairs} }}")
        };

        out.push('\n');
        emit_method_jsdoc(out, &m.docs, &m.params, "\t");
        out.push_str(&format!(
            "\t{name}({decl}): Promise<{ret}> {{\n",
            name = m.ts_name,
            decl = param_decl,
            ret = m.ret_ts,
        ));
        out.push_str(&channel_setup);
        out.push_str(&format!(
            "\t\treturn invoke(\"{cmd}\"{invoke_args});\n",
            cmd = m.command,
        ));
        out.push_str("\t}\n");
    }
    out.push_str("}\n");
}

fn capitalize(s: &str) -> String {
    let mut chars = s.chars();
    match chars.next() {
        Some(first) => first.to_uppercase().collect::<String>() + chars.as_str(),
        None => String::new(),
    }
}

fn emit_events_class(out: &mut String, ev: &EventApi) {
    out.push('\n');
    emit_jsdoc(out, &ev.docs, "");
    out.push_str(&format!("export class {} {{\n", ev.class_name));

    for e in &ev.events {
        out.push('\n');
        emit_jsdoc(out, &e.docs, "\t");
        out.push_str(&format!(
            "\ton{cap}(handler: (payload: {pl}) => void): Promise<UnlistenFn> {{\n",
            cap = capitalize(&e.ts_name),
            pl = e.payload_ts,
        ));
        out.push_str(&format!(
            "\t\treturn listen<{pl}>(\"{wire}\", handler);\n",
            pl = e.payload_ts,
            wire = e.wire,
        ));
        out.push_str("\t}\n");
    }
    out.push_str("}\n");
}

fn extract_attr_namespace(t: &ItemTrait, attr_name: &str) -> Option<String> {
    for attr in &t.attrs {
        if !attr.path().is_ident(attr_name) {
            continue;
        }
        let Meta::List(list) = &attr.meta else {
            continue;
        };
        let parser = syn::punctuated::Punctuated::<Meta, syn::Token![,]>::parse_terminated;
        let metas = match syn::parse::Parser::parse2(parser, list.tokens.clone()) {
            Ok(m) => m,
            Err(_) => continue,
        };
        for meta in metas {
            if let Meta::NameValue(nv) = meta {
                if nv.path.is_ident("namespace") {
                    if let Expr::Lit(ExprLit {
                        lit: Lit::Str(s), ..
                    }) = nv.value
                    {
                        return Some(s.value());
                    }
                }
            }
        }
    }
    None
}

fn has_attr(attrs: &[syn::Attribute], name: &str) -> bool {
    attrs.iter().any(|a| a.path().is_ident(name))
}

fn extract_docs(attrs: &[syn::Attribute]) -> Vec<String> {
    let mut lines = Vec::new();
    for attr in attrs {
        if !attr.path().is_ident("doc") {
            continue;
        }
        let syn::Meta::NameValue(nv) = &attr.meta else {
            continue;
        };
        let syn::Expr::Lit(syn::ExprLit {
            lit: syn::Lit::Str(s),
            ..
        }) = &nv.value
        else {
            continue;
        };
        let text = s.value();
        let trimmed = text.strip_prefix(' ').unwrap_or(&text);
        lines.push(trimmed.to_string());
    }
    lines
}

fn format_ts(content: &str) -> String {
    let mut out = String::new();
    let mut depth: i32 = 0;
    for raw_line in content.lines() {
        let trimmed = raw_line.trim_start();
        let starts_close = trimmed.starts_with('}') || trimmed.starts_with(']');
        let line_depth = if starts_close {
            (depth - 1).max(0)
        } else {
            depth
        };
        if !trimmed.is_empty() {
            for _ in 0..line_depth {
                out.push('\t');
            }
            if trimmed.starts_with('*') {
                out.push(' ');
            }
            out.push_str(trimmed);
        }
        out.push('\n');
        let opens = trimmed.matches('{').count() as i32 + trimmed.matches('[').count() as i32;
        let closes = trimmed.matches('}').count() as i32 + trimmed.matches(']').count() as i32;
        depth = (depth + opens - closes).max(0);
    }
    out
}

fn emit_jsdoc(out: &mut String, docs: &[String], indent: &str) {
    if docs.is_empty() {
        return;
    }
    if docs.len() == 1 {
        out.push_str(&format!("{indent}/** {} */\n", docs[0]));
        return;
    }
    out.push_str(&format!("{indent}/**\n"));
    for line in docs {
        out.push_str(&format!("{indent} * {line}\n"));
    }
    out.push_str(&format!("{indent} */\n"));
}

fn emit_method_jsdoc(out: &mut String, method_docs: &[String], params: &[Param], indent: &str) {
    let has_method = !method_docs.is_empty();
    let has_params = params.iter().any(|p| !p.docs.is_empty());
    if !has_method && !has_params {
        return;
    }
    if has_method && !has_params && method_docs.len() == 1 {
        out.push_str(&format!("{indent}/** {} */\n", method_docs[0]));
        return;
    }
    out.push_str(&format!("{indent}/**\n"));
    for line in method_docs {
        out.push_str(&format!("{indent} * {line}\n"));
    }
    for p in params {
        if p.docs.is_empty() {
            continue;
        }
        let summary = p.docs.join(" ");
        out.push_str(&format!("{indent} * @param {} - {summary}\n", p.name));
    }
    out.push_str(&format!("{indent} */\n"));
}

fn snake_to_camel(s: &str) -> String {
    let mut out = String::with_capacity(s.len());
    let mut upper = false;
    for c in s.chars() {
        if c == '_' {
            upper = true;
        } else if upper {
            out.extend(c.to_uppercase());
            upper = false;
        } else {
            out.push(c);
        }
    }
    out
}

fn rust_type_to_string(ty: &Type) -> String {
    use quote::ToTokens;
    let raw = ty.to_token_stream().to_string();
    let bytes = raw.as_bytes();
    let mut out = String::with_capacity(raw.len());
    for (i, &b) in bytes.iter().enumerate() {
        if b != b' ' {
            out.push(b as char);
            continue;
        }
        let prev = bytes.get(i.wrapping_sub(1)).copied().unwrap_or(0);
        let next = bytes.get(i + 1).copied().unwrap_or(0);
        let prev_join = matches!(prev, b'<' | b'(' | b':' | b',' | b'&');
        let next_join = matches!(next, b'<' | b'>' | b'(' | b')' | b':' | b',' | b';');
        if prev_join || next_join {
            continue;
        }
        out.push(' ');
    }
    out
}

fn rust_type_to_ts(ty: &Type, imports: &mut BTreeSet<String>) -> String {
    match ty {
        Type::Tuple(t) if t.elems.is_empty() => "void".into(),
        Type::Path(p) => {
            let seg = p.path.segments.last().unwrap();
            let name = seg.ident.to_string();
            match name.as_str() {
                "String" | "str" => "string".into(),
                "bool" => "boolean".into(),
                "u8" | "u16" | "u32" | "u64" | "usize" | "i8" | "i16" | "i32" | "i64" | "isize"
                | "f32" | "f64" => "number".into(),
                "Vec" => {
                    let inner = first_generic(&seg.arguments)
                        .map(|t| rust_type_to_ts(t, imports))
                        .unwrap_or_else(|| "unknown".into());
                    format!("{inner}[]")
                }
                "Option" => {
                    let inner = first_generic(&seg.arguments)
                        .map(|t| rust_type_to_ts(t, imports))
                        .unwrap_or_else(|| "unknown".into());
                    format!("{inner} | null")
                }
                _ => {
                    imports.insert(name.clone());
                    name
                }
            }
        }
        _ => "unknown".into(),
    }
}

fn extract_result_inner_ts(ty: &Type, imports: &mut BTreeSet<String>) -> String {
    if let Type::Path(p) = ty {
        let seg = p.path.segments.last().unwrap();
        if seg.ident == "Result" || seg.ident == "RpcResult" {
            if let Some(inner) = first_generic(&seg.arguments) {
                return rust_type_to_ts(inner, imports);
            }
        }
    }
    rust_type_to_ts(ty, imports)
}

fn first_generic(args: &PathArguments) -> Option<&Type> {
    if let PathArguments::AngleBracketed(a) = args {
        for arg in &a.args {
            if let GenericArgument::Type(t) = arg {
                return Some(t);
            }
        }
    }
    None
}
