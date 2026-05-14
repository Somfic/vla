use proc_macro::TokenStream;
use quote::quote;
use syn::parse::Parser;
use syn::{
    parse_macro_input, punctuated::Punctuated, DeriveInput, Expr, ExprLit, ItemTrait, Lit, Meta,
    Token, TraitItem,
};

#[proc_macro_attribute]
pub fn vla_type(_attr: TokenStream, item: TokenStream) -> TokenStream {
    let input = parse_macro_input!(item as DeriveInput);
    quote! {
        #[derive(Debug, Clone, ::serde::Serialize, ::serde::Deserialize, ::ts_rs::TS)]
        #[ts(export, export_to = "../bindings/_per_type/")]
        #input
    }
    .into()
}

#[proc_macro_attribute]
pub fn vla_api(attr: TokenStream, item: TokenStream) -> TokenStream {
    let parser = Punctuated::<Meta, Token![,]>::parse_terminated;
    let metas = match parser.parse(attr) {
        Ok(m) => m,
        Err(e) => return e.to_compile_error().into(),
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

    let namespace = match namespace {
        Some(n) => n,
        None => {
            return syn::Error::new(
                proc_macro2::Span::call_site(),
                "#[vla_api] requires `namespace = \"...\"`",
            )
            .to_compile_error()
            .into();
        }
    };

    let mut input = parse_macro_input!(item as ItemTrait);
    for item in &mut input.items {
        if let TraitItem::Fn(method) = item {
            let camel = snake_to_camel(&method.sig.ident.to_string());
            let attr: syn::Attribute = syn::parse_quote!(#[method(name = #camel)]);
            method.attrs.insert(0, attr);
        }
    }

    quote! {
        #[::jsonrpsee::proc_macros::rpc(server, namespace = #namespace)]
        #input
    }
    .into()
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
