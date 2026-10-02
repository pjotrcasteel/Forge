const familyExamples={
delta:{kicker:"DELTA · SEMANTIC CHANGE",title:"Turn before/after state into a typed explanation.",description:"Only participating domain state becomes part of the result.",outputLabel:"RESULT",outputTitle:"CustomerDelta.Between(before, after)",state:"typed",metrics:[["Changed","2"],["Unchanged","1"],["Hidden I/O","0"],["Runtime reflection","0"]],guardrail:"Generated comparison follows declared domain semantics.",ops:[["update","Email","before@example.com → after@example.com","EmailChange"],["update","Address.City","Utrecht → Delft","nested path"],["preserve","Id","logical identity unchanged","equivalent"],["preserve","LastObservedAt","ignored by domain semantics","DeltaIgnore"]],code:"var delta = CustomerDelta.Between(before, after);\n\ndelta.EmailChange;\ndelta.AddressDelta;\ndelta.Changes;"},
sync:{kicker:"SYNC · DESIRED STATE",title:"Turn current and desired state into an explicit transition plan.",description:"Add, update, remove and preserve are first-class result categories.",outputLabel:"PLAN",outputTitle:"OrderItemSync.Plan(current, desired)",state:"planned",metrics:[["Added","1"],["Updated","1"],["Removed","1"],["Preserved","0"]],guardrail:"Forge calculates the plan; your application still owns execution.",ops:[["update","B","quantity 1 → 2","typed Delta"],["add","D","present only in desired","desired key"],["remove","C","absent from complete desired state","current key"],["preserve","A","semantically equivalent","unchanged"]],code:"var plan = OrderItemSync.Plan(current, desired);\n\nplan.Added;\nplan.Updated;\nplan.Removed;\nplan.Unchanged;"},
parse:{kicker:"PARSE · STRUCTURED EXPECTATION",title:"Match dynamic JSON without throwing away the important structure.",description:"Generated values can stay dynamic while relationships and business-relevant fields remain explicit.",outputLabel:"MATCH",outputTitle:"JsonMatcher.Match(expected, actual)",state:"matched",metrics:[["Matched","4"],["Mismatches","0"],["Ignored","1"],["Custom matchers","0"]],guardrail:"Parse compares structure and declared expectations; it does not normalize away intent.",ops:[["add","$.id","<Guid>","dynamic type"],["add","$.state","<OneOf:pending|active>","allowed values"],["update","$.serviceId","<Capture:serviceId>","capture"],["preserve","$.dependency.serviceId","<Same:serviceId>","relationship"]],code:"JsonAssert.Matches(\n    expectedJson,\n    actualJson);"},
decide:{kicker:"DECIDE · EXPLAINABLE CHOICE",title:"Choose one valid course of action from a bounded strategy space.",description:"Only explicitly admitted strategies compete; proposals are frozen before a deterministic policy selects the plan.",outputLabel:"DECISION",outputTitle:"comparison.DecideAsync(context, policy, ...)",state:"selected",metrics:[["Admitted","2"],["Applicable","2"],["Selected","1"],["Executed","0"]],guardrail:"Forge selects and explains a plan; your application still owns every side effect.",ops:[["update","fast-lane","applicable · score 92","selected"],["preserve","balanced","applicable · alternate proposal","candidate"],["remove","economy","outside this strategy space","not evaluated"],["add","decision evidence","explain · digest · diff","portable"]],code:"var comparison = await space.CompareAsync(\n    context, cancellationToken);\n\nvar decision = await comparison.DecideAsync(\n    context, policy, cancellationToken);"}
};

const samples={
delta:{title:"C# · Forge.Delta",noteTitle:"Start with semantic change.",noteBody:"Delta is independently useful; Sync only needs it when planning updates.",link:"https://github.com/pjotrcasteel/Forge#your-first-delta",code:"dotnet add package Forge.Delta --version 1.20.0\n\nusing Forge.Delta;\n\n[GenerateDelta]\npublic sealed record Customer(Guid Id, string Name, string? Email);\n\nvar delta = CustomerDelta.Between(before, after);"},
sync:{title:"C# · Forge.Sync",noteTitle:"Use Sync when identity and desired state matter.",noteBody:"Replace and Upsert semantics are explicit, and every update can carry its typed Delta.",link:"https://github.com/pjotrcasteel/Forge#your-first-reconciliation",code:"dotnet add package Forge.Sync --version 1.20.0\n\nusing Forge.Sync;\n\n[GenerateSync(nameof(OrderItem.Id))]\npublic sealed record OrderItem(string Id, string Product, int Quantity);\n\nvar plan = OrderItemSync.Plan(current, desired);"},
parse:{title:"C# · Forge.Parse",noteTitle:"Keep dynamic values dynamic without weakening the assertion.",noteBody:"Use placeholders, captures and partial matching to state exactly what matters.",link:"https://www.nuget.org/packages/Forge.Parse",code:"dotnet add package Forge.Parse --version 1.20.0\n\nusing Forge.Parse;\n\nJsonAssert.Matches(expectedJson, actualJson);"},
decide:{title:"C# · Forge.Decide",noteTitle:"Bound the candidates before you select a plan.",noteBody:"Strategies propose without side effects; an explicit policy chooses from frozen evidence.",link:"./decide.html",code:"dotnet add package Forge.Decide --version 1.20.0\n\nusing Forge.Decide;\n\nvar comparison = await space.CompareAsync(\n    context, cancellationToken);\nvar decision = await comparison.DecideAsync(\n    context, policy, cancellationToken);"},
reqnroll:{title:"C# · Forge.Parse.Reqnroll",noteTitle:"Keep the adapter thin.",noteBody:"Reqnroll DataTables become Parse expectations; matching semantics stay in Forge.Parse.",link:"https://www.nuget.org/packages/Forge.Parse.Reqnroll",code:"dotnet add package Forge.Parse.Reqnroll --version 1.20.0\n\nusing Forge.Parse.Reqnroll;\n\nvar result = table.MatchJsonObject(actualJson, expandColumnPaths: true);"}
};

const byId=id=>document.getElementById(id);
function activate(selector,current){document.querySelectorAll(selector).forEach(button=>{const active=button===current;button.classList.toggle("active",active);button.setAttribute("aria-selected",String(active));});}
async function copyText(value,button,idle){try{await navigator.clipboard.writeText(value);const target=button.querySelector(".copy-label")||button;const fallback=idle||target.textContent;target.textContent="Copied";setTimeout(()=>target.textContent=fallback,1300);}catch{const target=button.querySelector(".copy-label")||button;target.textContent="Select";}}

function hydrateDecideFamily(){
  if(document.querySelector('link[href="./decide-home.css"]')===null){
    const stylesheet=document.createElement("link");
    stylesheet.rel="stylesheet";
    stylesheet.href="./decide-home.css";
    document.head.appendChild(stylesheet);
  }

  document.title="Forge — A family of four focused .NET tools";
  const description=document.querySelector('meta[name="description"]');
  if(description){description.content="Forge is an open-source .NET 10 library family: Delta for semantic differences, Sync for desired-state reconciliation, Parse for structured expectations, and Decide for bounded explainable strategy decisions.";}

  const heroTitle=document.querySelector(".family-hero h1 span");
  if(heroTitle){heroTitle.textContent="Four focused tools.";}
  const heroLead=document.querySelector(".family-hero .hero-lead");
  if(heroLead&&!heroLead.textContent.includes("Decide")){heroLead.insertAdjacentHTML("beforeend"," <em>Decide</em> chooses which valid course of action becomes the plan, and preserves why.");}
  const heroInstalls=document.querySelector(".hero-installs");
  if(heroInstalls&&!heroInstalls.querySelector('[data-copy*="Forge.Decide"]')){heroInstalls.insertAdjacentHTML("beforeend",'<button class="install install-compact" type="button" data-copy="dotnet add package Forge.Decide --version 1.20.0"><code>Forge.Decide</code><span class="copy-label">Copy</span></button>');}

  const familyBoard=document.querySelector(".family-board-body");
  if(familyBoard&&!familyBoard.querySelector(".decide-node")){
    const syncNode=familyBoard.querySelector(".sync-node");
    syncNode?.insertAdjacentHTML("afterend",'<article class="family-node decide-node"><span class="family-symbol">◆</span><div><small>FORGE.DECIDE</small><strong>Strategy decisions</strong><p>context → chosen plan</p></div></article>');
  }
  const foot=document.querySelectorAll(".blueprint-foot span");
  if(foot[0]){foot[0].innerHTML="<strong>4</strong> core packages";}
  if(foot[1]){foot[1].innerHTML="<strong>focused</strong> optional extensions";}

  const signals=document.querySelector(".signals");
  if(signals&&!Array.from(signals.children).some(node=>node.textContent==="Forge.Decide")){signals.children[2]?.insertAdjacentHTML("afterend","<span>Forge.Decide</span>");}

  const packageGrid=document.querySelector(".family-package-grid");
  if(packageGrid&&!packageGrid.querySelector(".decide-card")){
    packageGrid.insertAdjacentHTML("beforeend",'<article class="package-card decide-card reveal visible"><div class="package-top"><span class="package-mark">◆</span><span class="mono-label">FORGE.DECIDE</span></div><h3>Which valid course of action should become the plan?</h3><p>Bounded strategy spaces, side-effect-free proposals, deterministic selection, explanations, shadow policies and evidence diffing.</p><div class="package-question">context + alternatives → explainable decision</div><pre><code>var comparison = await space.CompareAsync(context, ct);\nvar decision = await comparison.DecideAsync(context, policy, ct);</code></pre><div class="package-actions"><a class="text-link" href="https://www.nuget.org/packages/Forge.Decide">NuGet →</a><a class="text-link muted-link" href="./decide.html">Functionality page →</a></div></article>');
  }

  const decisionStrip=document.querySelector(".decision-strip");
  if(decisionStrip&&!decisionStrip.textContent.includes("Decide")){decisionStrip.children[2]?.insertAdjacentHTML("afterend","<span>Strategy choice → <b>Decide</b></span>");}

  const scenarioPicker=document.querySelector(".scenario-picker");
  if(scenarioPicker&&!scenarioPicker.querySelector('[data-family="decide"]')){scenarioPicker.insertAdjacentHTML("beforeend",'<button class="scenario-button" role="tab" aria-selected="false" data-family="decide"><span>Forge.Decide</span><small>Select an explainable plan</small></button>');}

  const architectureGrid=document.querySelector(".architecture-grid");
  if(architectureGrid&&!architectureGrid.querySelector(".decide-architecture")){architectureGrid.insertAdjacentHTML("beforeend",'<article class="architecture-card decide-architecture"><span class="mono-label">INDEPENDENT CORE</span><h3>Decide freezes evidence before selection.</h3><p>Typed spaces control who may compete. Strategies propose plans. Selection stays explicit, deterministic and execution-free.</p><div class="dependency-rail"><b>Forge.Decide</b><i>→</i><span>application-owned plans</span></div></article>');}

  const codeTabs=document.querySelector(".code-tabs");
  if(codeTabs&&!codeTabs.querySelector('[data-code="decide"]')){codeTabs.children[2]?.insertAdjacentHTML("afterend",'<button class="code-tab" role="tab" aria-selected="false" data-code="decide">Forge.Decide</button>');}

  const useGrid=document.querySelector(".use-grid");
  if(useGrid&&!useGrid.querySelector(".decide-use")){useGrid.insertAdjacentHTML("beforeend",'<article class="use-card decide-use reveal visible"><span>◆</span><h3>Strategy decisions</h3><p>Restrict which approaches may compete, compare proposals before side effects, and retain evidence for why one plan was selected.</p></article>');}

  const resourceGrid=document.querySelector(".resource-grid");
  if(resourceGrid&&!resourceGrid.querySelector(".decide-resource")){resourceGrid.insertAdjacentHTML("afterbegin",'<a class="resource-card decide-resource reveal visible" href="./decide.html"><span>◆</span><div><h3>Forge.Decide functionality</h3><p>Strategy spaces, proposals, shadow selection, receipts, diffs and optional integrations.</p></div><b>↗</b></a>');}

  const footerLinks=document.querySelector(".footer-links");
  if(footerLinks&&!footerLinks.textContent.includes("Decide")){footerLinks.innerHTML=footerLinks.innerHTML.replace(" · <a href=\"./llms.txt\">"," · <a href=\"./decide.html\">Decide</a> · <a href=\"./llms.txt\">");}
}

hydrateDecideFamily();

if("IntersectionObserver" in window){const observer=new IntersectionObserver(entries=>entries.forEach(entry=>{if(entry.isIntersecting){entry.target.classList.add("visible");observer.unobserve(entry.target);}}),{threshold:.1});document.querySelectorAll(".reveal").forEach(el=>observer.observe(el));}else{document.querySelectorAll(".reveal").forEach(el=>el.classList.add("visible"));}
document.querySelectorAll("[data-copy]").forEach(button=>button.addEventListener("click",()=>copyText(button.dataset.copy,button,"Copy")));

function renderFamily(key){const data=familyExamples[key];byId("family-kicker").textContent=data.kicker;byId("family-title").textContent=data.title;byId("family-description").textContent=data.description;byId("family-output-label").textContent=data.outputLabel;byId("family-output-title").textContent=data.outputTitle;byId("family-state").textContent=data.state;data.metrics.forEach((metric,index)=>{byId("metric-label-"+(index+1)).textContent=metric[0];byId("metric-value-"+(index+1)).textContent=metric[1];});byId("family-guardrail").textContent=data.guardrail;byId("family-operation-list").innerHTML=data.ops.map(op=>"<div class=\"operation\"><span class=\"op-kind "+op[0]+"\">"+op[0]+"</span><div><strong>"+op[1]+"</strong><small>"+op[2]+"</small></div><code>"+op[3]+"</code></div>").join("");byId("copy-family-snippet").dataset.value=data.code;}
document.querySelectorAll("[data-family]").forEach(button=>button.addEventListener("click",()=>{activate("[data-family]",button);renderFamily(button.dataset.family);}));
byId("copy-family-snippet").addEventListener("click",event=>copyText(event.currentTarget.dataset.value,event.currentTarget,"Copy C#"));
renderFamily("delta");

function renderCode(key){const data=samples[key];byId("code-title").textContent=data.title;byId("code-example").textContent=data.code;byId("code-note-title").textContent=data.noteTitle;byId("code-note-body").textContent=data.noteBody;byId("code-note-link").href=data.link;byId("copy-code").dataset.value=data.code;}
document.querySelectorAll("[data-code]").forEach(button=>button.addEventListener("click",()=>{activate("[data-code]",button);renderCode(button.dataset.code);}));
byId("copy-code").addEventListener("click",event=>copyText(event.currentTarget.dataset.value,event.currentTarget,"Copy"));
renderCode("delta");
