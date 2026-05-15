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

#[proc_macro_attribute]
pub fn vla_api(attr: TokenStream, item: TokenStream) -> TokenStream {
    let namespace = match parse_namespace(attr, "vla_api") {
        Ok(n) => n,
        Err(ts) => return ts,
    };
    let _ = namespace;
    let input = parse_macro_input!(item as ItemTrait);
    quote! {
        #[::async_trait::async_trait]
        #input
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
