const scenarios={
replace:{kicker:"SYNC · REPLACE",title:"Complete desired state produces a complete structural plan.",description:"An item absent from desired state is intentionally classified as removed.",output:"OrderItemSync.Plan(current, desired)",state:"pure",stats:[1,1,1,0],guardrail:"No persistence or external calls occur while the plan is calculated.",ops:[["update","B","quantity 1 → 2","delta.QuantityChange"],["add","D","present only in desired","desired key"],["remove","C","absent from complete desired state","current key"],["preserve","A","semantically equivalent","unchanged"]],code:`var plan = OrderItemSync.Plan(current, desired);

plan.Added;
plan.Updated;
plan.Removed;
plan.Unchanged;`},
upsert:{kicker:"SYNC · UPSERT",title:"Partial desired state preserves omitted current items.",description:"Upsert makes partial-input semantics explicit so omission cannot silently become deletion.",output:"OrderItemSync.Plan(current, payload, SyncMode.Upsert)",state:"pure",stats:[1,1,0,1],guardrail:"Preserved is a distinct result category, not a hidden special case.",ops:[["update","B","quantity 1 → 2","payload key"],["add","D","new item in payload","payload key"],["preserve","C","omitted from partial payload","current-only"],["preserve","A","omitted from partial payload","current-only"]],code:`var plan = OrderItemSync.Plan(
    current,
    payload,
    SyncMode.Upsert);

plan.Preserved;`},
topology:{kicker:"TOPOLOGY · STRUCTURAL PHASES",title:"Node and edge changes are planned in graph-safe phases.",description:"Forge validates edges against their snapshot and exposes an execution-safe structural ordering.",output:"TopologySync.Plan(...)",state:"ordered",stats:[1,1,1,0],guardrail:"Forge calculates the phases; your application still decides how to execute each operation.",ops:[["wave","01","RemoveEdges","dependent edge first"],["wave","02","RemoveNodes","after edge removal"],["wave","03","AddNodes / UpdateNodes","node state before new edges"],["wave","04","UpdateEdges / AddEdges","relationships last"]],code:`var topology = TopologySync.Plan(
    currentNodes,
    desiredNodes,
    currentEdges,
    desiredEdges,
    nodeDefinition,
    edgeDefinition);

topology.RemoveEdges;
topology.RemoveNodes;
topology.AddNodes;`},
replan:{kicker:"REPLAN · EXECUTION AWARE",title:"A new plan must respect work that already crossed the execution boundary.",description:"Pending work can change freely; running work becomes an explicit conflict; completed semantic changes become compensation requirements.",output:"ExecutedReplanner.Replan(...)",state:"guarded",stats:[1,1,0,0],guardrail:"Execution state stays application-defined and strongly typed.",ops:[["preserve","done-12","completed → compensation required","Completed"],["update","pending-21","not started → replace safely","Pending"],["wave","running-08","semantic change → conflict","Running"],["add","new-31","newly required operation","Pending"]],code:`var result = ExecutedReplanner.Replan(
    trackedPlan,
    nextPlan,
    executionStates,
    classifier,
    cancellationToken);`}
};

const samples={
delta:{title:"C# · Forge.Delta",noteTitle:"Start with the semantic change itself.",noteBody:"Generated code uses direct property access and your declared equality semantics.",link:"https://github.com/pjotrcasteel/Forge#your-first-delta",code:`dotnet add package Forge.Delta --version 1.20.0

using Forge.Delta;

[GenerateDelta]
public sealed record Customer(
    Guid Id,
    string Name,
    string? Email);

var delta = CustomerDelta.Between(before, after);

if (delta.EmailChange.HasChanged)
{
    Console.WriteLine(
        $"${delta.EmailChange.Before} -> ${delta.EmailChange.After}");
}`},
sync:{title:"C# · Forge.Sync",noteTitle:"Use Replace for complete desired state.",noteBody:"Choose Upsert explicitly when omitted current items must be preserved.",link:"https://github.com/pjotrcasteel/Forge#your-first-reconciliation",code:`dotnet add package Forge.Sync --version 1.20.0

using Forge.Sync;

[GenerateSync(nameof(OrderItem.Id))]
public sealed record OrderItem(
    string Id,
    string Product,
    int Quantity);

var plan = OrderItemSync.Plan(current, desired);

plan.Added;
plan.Updated;
plan.Removed;
plan.Unchanged;`},
dependencies:{title:"C# · dependency planning",noteTitle:"Add ordering only when ordering is part of correctness.",noteBody:"DependencyPlanner detects cycles and exposes create/delete waves without executing any work.",link:"https://github.com/pjotrcasteel/Forge#dependency-aware-planning",code:`var dependencyPlan = DependencyPlanner.Plan(
    operations,
    static item => item.Id,
    static item => item.DependsOn);

foreach (var wave in dependencyPlan.CreateWaves)
{
    // Safe after every previous wave completed.
}

foreach (var wave in dependencyPlan.DeleteWaves)
{
    // Dependent-first order for teardown.
}`}
};

const byId=id=>document.getElementById(id);
function activate(selector,current){document.querySelectorAll(selector).forEach(button=>{const active=button===current;button.classList.toggle("active",active);button.setAttribute("aria-selected",String(active))})}
async function copyText(value,button,idle){try{await navigator.clipboard.writeText(value);const target=button.querySelector(".copy-label")||button;const fallback=idle||target.textContent;target.textContent="Copied";setTimeout(()=>target.textContent=fallback,1300)}catch{const target=button.querySelector(".copy-label")||button;target.textContent="Select"}}

if("IntersectionObserver" in window){const observer=new IntersectionObserver(entries=>entries.forEach(entry=>{if(entry.isIntersecting){entry.target.classList.add("visible");observer.unobserve(entry.target)}}),{threshold:.1});document.querySelectorAll(".reveal").forEach(el=>observer.observe(el))}else{document.querySelectorAll(".reveal").forEach(el=>el.classList.add("visible"))}
document.querySelectorAll("[data-copy]").forEach(button=>button.addEventListener("click",()=>copyText(button.dataset.copy,button,"Copy")));

function renderScenario(key){
 const data=scenarios[key];
 byId("scenario-kicker").textContent=data.kicker;
 byId("scenario-title").textContent=data.title;
 byId("scenario-description").textContent=data.description;
 byId("output-title").textContent=data.output;
 byId("output-state").textContent=data.state;
 ["added","updated","removed","preserved"].forEach((name,index)=>byId("stat-"+name).textContent=String(data.stats[index]));
 byId("output-guardrail").textContent=data.guardrail;
 byId("operation-list").innerHTML=data.ops.map(op=>`<div class="operation"><span class="op-kind ${op[0]}">${op[0]}</span><div><strong>${op[1]}</strong><small>${op[2]}</small></div><code>${op[3]}</code></div>`).join("");
 byId("copy-snippet").dataset.value=data.code;
}
document.querySelectorAll("[data-scenario]").forEach(button=>button.addEventListener("click",()=>{activate("[data-scenario]",button);renderScenario(button.dataset.scenario)}));
byId("copy-snippet").addEventListener("click",event=>copyText(event.currentTarget.dataset.value,event.currentTarget,"Copy C#"));
renderScenario("replace");

function renderCode(key){
 const data=samples[key];
 byId("code-title").textContent=data.title;
 byId("code-example").textContent=data.code;
 byId("code-note-title").textContent=data.noteTitle;
 byId("code-note-body").textContent=data.noteBody;
 byId("code-note-link").href=data.link;
 byId("copy-code").dataset.value=data.code;
}
document.querySelectorAll("[data-code]").forEach(button=>button.addEventListener("click",()=>{activate("[data-code]",button);renderCode(button.dataset.code)}));
byId("copy-code").addEventListener("click",event=>copyText(event.currentTarget.dataset.value,event.currentTarget,"Copy"));
renderCode("delta");