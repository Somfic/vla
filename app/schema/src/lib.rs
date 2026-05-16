use proc_macro::TokenStream;
use quote::quote;
use syn::parse::Parser;
use syn::{
    parse_macro_input, punctuated::Punctuated, DeriveInput, Expr, ExprLit, ItemTrait, Lit, Meta,
    Token,
};

#[proc_macro_attribute]
pub fn vla_type(_attr: TokenStream, item: TokenStream) -> TokenStream {
    let input = parse_macro_input!(item as DeriveInput);
    quote! {
        #[derive(Debug, Clone, ::serde::Serialize, ::serde::Deserialize, ::ts_rs::TS)]
        #[ts(export, export_to = "_per_type/")]
        #input
    }
    .into()
}

/// Parses `namespace = "..."` from an attribute macro's argument list.
/// Returns the namespace string, or a compile-error token stream to emit.
fn parse_namespace(attr: TokenStream, macro_name: &str) -> Result<String, TokenStream> {
    let parser = Punctuated::<Meta, Token![,]>::parse_terminated;
    let metas = match parser.parse(attr) {
        Ok(m) => m,
        Err(e) => return Err(e.to_compile_error().into()),
    };

    let mut namespace: Option<String> = None;
    for meta in metas {
        if let Meta::NameValue(nv) = meta {
            if nv.path.is_ident("namespace") {
                if let Expr::Lit(ExprLit {
                    lit: Lit::Str(s), ..
                }) = nv.value
                {
                    namespace = Some(s.value());
                }
            }
        }
    }

    namespace.ok_or_else(|| {
        syn::Error::new(
            proc_macro2::Span::call_site(),
            format!("#[{macro_name}] requires `namespace = \"...\"`"),
        )
        .to_compile_error()
        .into()
    })
}

/// On a `trait` this declares an API namespace: it requires
/// `namespace = "..."` (codegen reads it from source; missing it should fail
/// loudly) and injects `#[async_trait]`. On an `impl ... for App` block it
/// takes no arguments and is just shorthand for `#[async_trait]`.
#[proc_macro_attribute]
pub fn vla_api(attr: TokenStream, item: TokenStream) -> TokenStream {
    let input = parse_macro_input!(item as syn::Item);
    match input {
        syn::Item::Trait(t) => {
            if let Err(ts) = parse_namespace(attr, "vla_api") {
                return ts;
            }
            quote! {
                #[::async_trait::async_trait]
                #t
            }
            .into()
        }
        syn::Item::Impl(i) => quote! {
            #[::async_trait::async_trait]
            #i
        }
        .into(),
        other => syn::Error::new_spanned(other, "#[vla_api] expects a trait or impl block")
            .to_compile_error()
            .into(),
    }
}

/// Marks an `async fn <name>(app: crate::app::App)` as a background service.
/// The async body is rewritten into a sync fn that spawns it as a detached
/// task on Tauri's runtime, so authors just write the loop — no manual
/// `spawn`. Codegen collects every `#[vla_service]` into
/// `_generated::start_services` (it reads the fn from source, like
/// `#[vla_api]`), which the Tauri setup hook calls once on startup.
#[proc_macro_attribute]
pub fn vla_service(_attr: TokenStream, item: TokenStream) -> TokenStream {
    let func = parse_macro_input!(item as syn::ItemFn);
    if func.sig.asyncness.is_none() {
        return syn::Error::new_spanned(&func.sig, "#[vla_service] requires an `async fn`")
            .to_compile_error()
            .into();
    }
    let attrs = &func.attrs;
    let vis = &func.vis;
    let ident = &func.sig.ident;
    let inputs = &func.sig.inputs;
    let block = &func.block;
    // The spawned task must be `'static`, so it can't borrow the caller's
    // `&App`. Each borrowed param is cloned into an owned binding (a cheap
    // `Arc` clone) that the `async move` captures.
    let owned: Vec<_> = inputs
        .iter()
        .filter_map(|arg| {
            let syn::FnArg::Typed(pat) = arg else {
                return None;
            };
            let syn::Pat::Ident(p) = &*pat.pat else {
                return None;
            };
            let name = &p.ident;
            Some(quote! { let #name = (*#name).clone(); })
        })
        .collect();
    quote! {
        #(#attrs)*
        #vis fn #ident(#inputs) {
            #(#owned)*
            ::tauri::async_runtime::spawn(async move #block);
        }
    }
    .into()
}

/// Declares a namespace of backend→frontend events. The annotated trait is a
/// pure, bodyless manifest consumed by the schema codegen — it is never
/// implemented and is erased at compile time (this macro expands to nothing).
#[proc_macro_attribute]
pub fn vla_events(attr: TokenStream, item: TokenStream) -> TokenStream {
    let namespace = match parse_namespace(attr, "vla_events") {
        Ok(n) => n,
        Err(ts) => return ts,
    };
    let _ = namespace;
    // Enforce it parses as a well-formed trait (surfaces syntax errors), then
    // emit nothing: codegen reads the trait from source, not from expansion.
    let _input = parse_macro_input!(item as ItemTrait);
    TokenStream::new()
}
