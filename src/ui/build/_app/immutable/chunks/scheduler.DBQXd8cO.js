function A(){}function M(t,e){for(const n in e)t[n]=e[n];return t}function T(t){return t()}function tt(){return Object.create(null)}function W(t){t.forEach(T)}function q(t){return typeof t=="function"}function et(t,e){return t!=t?e==e:t!==e||t&&typeof t=="object"||typeof t=="function"}function nt(t,e){return t!=t?e==e:t!==e}function it(t){return Object.keys(t).length===0}function C(t,...e){if(t==null){for(const i of e)i(void 0);return A}const n=t.subscribe(...e);return n.unsubscribe?()=>n.unsubscribe():n}function rt(t){let e;return C(t,n=>e=n)(),e}function st(t,e,n){t.$$.on_destroy.push(C(e,n))}function ct(t,e,n,i){if(t){const r=j(t,e,n,i);return t[0](r)}}function j(t,e,n,i){return t[1]&&i?M(n.ctx.slice(),t[1](i(e))):n.ctx}function ot(t,e,n,i){if(t[2]&&i){const r=t[2](i(n));if(e.dirty===void 0)return r;if(typeof r=="object"){const o=[],s=Math.max(e.dirty.length,r.length);for(let l=0;l<s;l+=1)o[l]=e.dirty[l]|r[l];return o}return e.dirty|r}return e.dirty}function lt(t,e,n,i,r,o){if(r){const s=j(e,n,i,o);t.p(s,r)}}function at(t){if(t.ctx.length>32){const e=[],n=t.ctx.length/32;for(let i=0;i<n;i++)e[i]=-1;return e}return-1}function ut(t){const e={};for(const n in t)n[0]!=="$"&&(e[n]=t[n]);return e}function ft(t,e){const n={};e=new Set(e);for(const i in t)!e.has(i)&&i[0]!=="$"&&(n[i]=t[i]);return n}function _t(t){const e={};for(const n in t)e[n]=!0;return e}function dt(t){return t==null?"":t}function ht(t,e,n){return t.set(n),e}function mt(t){return t&&q(t.destroy)?t.destroy:A}let y=!1;function pt(){y=!0}function yt(){y=!1}function B(t,e,n,i){for(;t<e;){const r=t+(e-t>>1);n(r)<=i?t=r+1:e=r}return t}function I(t){if(t.hydrate_init)return;t.hydrate_init=!0;let e=t.childNodes;if(t.nodeName==="HEAD"){const c=[];for(let a=0;a<e.length;a++){const u=e[a];u.claim_order!==void 0&&c.push(u)}e=c}const n=new Int32Array(e.length+1),i=new Int32Array(e.length);n[0]=-1;let r=0;for(let c=0;c<e.length;c++){const a=e[c].claim_order,u=(r>0&&e[n[r]].claim_order<=a?r+1:B(1,r,D=>e[n[D]].claim_order,a))-1;i[c]=n[u]+1;const k=u+1;n[k]=c,r=Math.max(k,r)}const o=[],s=[];let l=e.length-1;for(let c=n[r]+1;c!=0;c=i[c-1]){for(o.push(e[c-1]);l>=c;l--)s.push(e[l]);l--}for(;l>=0;l--)s.push(e[l]);o.reverse(),s.sort((c,a)=>c.claim_order-a.claim_order);for(let c=0,a=0;c<s.length;c++){for(;a<o.length&&s[c].claim_order>=o[a].claim_order;)a++;const u=a<o.length?o[a]:null;t.insertBefore(s[c],u)}}function L(t,e){t.appendChild(e)}function H(t,e){if(y){for(I(t),(t.actual_end_child===void 0||t.actual_end_child!==null&&t.actual_end_child.parentNode!==t)&&(t.actual_end_child=t.firstChild);t.actual_end_child!==null&&t.actual_end_child.claim_order===void 0;)t.actual_end_child=t.actual_end_child.nextSibling;e!==t.actual_end_child?(e.claim_order!==void 0||e.parentNode!==t)&&t.insertBefore(e,t.actual_end_child):t.actual_end_child=e.nextSibling}else(e.parentNode!==t||e.nextSibling!==null)&&t.appendChild(e)}function bt(t,e,n){y&&!n?H(t,e):(e.parentNode!==t||e.nextSibling!=n)&&t.insertBefore(e,n||null)}function F(t){t.parentNode&&t.parentNode.removeChild(t)}function gt(t,e){for(let n=0;n<t.length;n+=1)t[n]&&t[n].d(e)}function S(t){return document.createElement(t)}function U(t){return document.createElementNS("http://www.w3.org/2000/svg",t)}function v(t){return document.createTextNode(t)}function xt(){return v(" ")}function wt(){return v("")}function E(t,e,n,i){return t.addEventListener(e,n,i),()=>t.removeEventListener(e,n,i)}function G(t,e,n){n==null?t.removeAttribute(e):t.getAttribute(e)!==n&&t.setAttribute(e,n)}const J=["width","height"];function vt(t,e){const n=Object.getOwnPropertyDescriptors(t.__proto__);for(const i in e)e[i]==null?t.removeAttribute(i):i==="style"?t.style.cssText=e[i]:i==="__value"?t.value=t[i]=e[i]:n[i]&&n[i].set&&J.indexOf(i)===-1?t[i]=e[i]:G(t,i,e[i])}function kt(t){return t.dataset.svelteH}function Et(t){return Array.from(t.childNodes)}function K(t){t.claim_info===void 0&&(t.claim_info={last_index:0,total_claimed:0})}function O(t,e,n,i,r=!1){K(t);const o=(()=>{for(let s=t.claim_info.last_index;s<t.length;s++){const l=t[s];if(e(l)){const c=n(l);return c===void 0?t.splice(s,1):t[s]=c,r||(t.claim_info.last_index=s),l}}for(let s=t.claim_info.last_index-1;s>=0;s--){const l=t[s];if(e(l)){const c=n(l);return c===void 0?t.splice(s,1):t[s]=c,r?c===void 0&&t.claim_info.last_index--:t.claim_info.last_index=s,l}}return i()})();return o.claim_order=t.claim_info.total_claimed,t.claim_info.total_claimed+=1,o}function z(t,e,n,i){return O(t,r=>r.nodeName===e,r=>{const o=[];for(let s=0;s<r.attributes.length;s++){const l=r.attributes[s];n[l.name]||o.push(l.name)}o.forEach(s=>r.removeAttribute(s))},()=>i(e))}function Nt(t,e,n){return z(t,e,n,S)}function At(t,e,n){return z(t,e,n,U)}function Q(t,e){return O(t,n=>n.nodeType===3,n=>{const i=""+e;if(n.data.startsWith(i)){if(n.data.length!==i.length)return n.splitText(i.length)}else n.data=i},()=>v(e),!0)}function Ct(t){return Q(t," ")}function jt(t,e){e=""+e,t.data!==e&&(t.data=e)}function St(t,e,n,i){n==null?t.style.removeProperty(e):t.style.setProperty(e,n,"")}let m;function R(){if(m===void 0){m=!1;try{typeof window<"u"&&window.parent&&window.parent.document}catch{m=!0}}return m}function Ot(t,e){getComputedStyle(t).position==="static"&&(t.style.position="relative");const i=S("iframe");i.setAttribute("style","display: block; position: absolute; top: 0; left: 0; width: 100%; height: 100%; overflow: hidden; border: 0; opacity: 0; pointer-events: none; z-index: -1;"),i.setAttribute("aria-hidden","true"),i.tabIndex=-1;const r=R();let o;return r?(i.src="data:text/html,<script>onresize=function(){parent.postMessage(0,'*')}<\/script>",o=E(window,"message",s=>{s.source===i.contentWindow&&e()})):(i.src="about:blank",i.onload=()=>{o=E(i.contentWindow,"resize",e),e()}),L(t,i),()=>{(r||o&&i.contentWindow)&&o(),F(i)}}function zt(t,e,n){t.classList.toggle(e,!!n)}function V(t,e,{bubbles:n=!1,cancelable:i=!1}={}){return new CustomEvent(t,{detail:e,bubbles:n,cancelable:i})}function Pt(t,e){return new t(e)}let p;function b(t){p=t}function f(){if(!p)throw new Error("Function called outside component initialization");return p}function Dt(t){f().$$.on_mount.push(t)}function Mt(t){f().$$.after_update.push(t)}function Tt(t){f().$$.on_destroy.push(t)}function Wt(){const t=f();return(e,n,{cancelable:i=!1}={})=>{const r=t.$$.callbacks[e];if(r){const o=V(e,n,{cancelable:i});return r.slice().forEach(s=>{s.call(t,o)}),!o.defaultPrevented}return!0}}function qt(t,e){return f().$$.context.set(t,e),e}function Bt(t){return f().$$.context.get(t)}function It(t){return f().$$.context.has(t)}function Lt(t,e){const n=t.$$.callbacks[e.type];n&&n.slice().forEach(i=>i.call(this,e))}const h=[],N=[];let d=[];const x=[],P=Promise.resolve();let w=!1;function X(){w||(w=!0,P.then(Z))}function Ht(){return X(),P}function Y(t){d.push(t)}function Ft(t){x.push(t)}const g=new Set;let _=0;function Z(){if(_!==0)return;const t=p;do{try{for(;_<h.length;){const e=h[_];_++,b(e),$(e.$$)}}catch(e){throw h.length=0,_=0,e}for(b(null),h.length=0,_=0;N.length;)N.pop()();for(let e=0;e<d.length;e+=1){const n=d[e];g.has(n)||(g.add(n),n())}d.length=0}while(h.length);for(;x.length;)x.pop()();w=!1,g.clear(),b(t)}function $(t){if(t.fragment!==null){t.update(),W(t.before_update);const e=t.dirty;t.dirty=[-1],t.fragment&&t.fragment.p(t.ctx,e),t.after_update.forEach(Y)}}function Ut(t){const e=[],n=[];d.forEach(i=>t.indexOf(i)===-1?e.push(i):n.push(i)),n.forEach(i=>i()),d=e}export{Lt as $,Mt as A,St as B,N as C,Pt as D,Ht as E,tt as F,Z as G,it as H,Y as I,Ut as J,p as K,b as L,T as M,h as N,X as O,pt as P,yt as Q,rt as R,zt as S,E as T,Bt as U,mt as V,qt as W,ht as X,Wt as Y,nt as Z,Tt as _,C as a,M as a0,vt as a1,ft as a2,ut as a3,kt as a4,Ot as a5,_t as a6,It as a7,dt as a8,gt as a9,Ft as aa,xt as b,Nt as c,Et as d,S as e,Q as f,F as g,Ct as h,q as i,bt as j,H as k,jt as l,st as m,A as n,ct as o,at as p,ot as q,W as r,et as s,v as t,lt as u,U as v,At as w,G as x,Dt as y,wt as z};
