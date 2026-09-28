const projects={
  causalia:{
    index:"01",status:"LIVE",symbol:"◎",category:"DETERMINISTIC SIMULATION",name:"Causalia",
    promise:"Explore what can happen.",
    description:"Turn rare concurrency, retry and distributed-system failures into deterministic, replayable .NET tests.",
    boundary:"Models controlled failure semantics; real provider integration tests still matter.",
    className:"causalia-focus",
    actions:'<a class="button button-small" href="https://pjotrcasteel.github.io/Causalia/">Visit Causalia</a><a class="text-link" href="https://github.com/pjotrcasteel/Causalia">Source ↗</a>'
  },
  forge:{
    index:"02",status:"LIVE",symbol:"⌁",category:"TYPED CHANGE + PLANNING",name:"Forge",
    promise:"Plan what should happen.",
    description:"Make semantic differences, desired-state reconciliation, dependencies, manifests and replanning explicit without handing execution to the library.",
    boundary:"Forge calculates and validates transitions; application-owned code retains policy, persistence and side effects.",
    className:"forge-focus",
    actions:'<a class="button button-small" href="https://pjotrcasteel.github.io/Forge/">Visit Forge</a><a class="text-link" href="https://pjotrcasteel.github.io/Forge/service-provisioning.html">2-minute production story ↗</a>'
  },
  gorm:{
    index:"03",status:"NEXT",symbol:"◇",category:"GRAPH PERSISTENCE",name:"GORM",
    promise:"Model how things connect.",
    description:"Graph-oriented data access and persistence around nodes, relationships and explicit SQL graph behavior.",
    boundary:"GORM should own graph mapping and persistence mechanics—not application domain policy.",
    className:"gorm-focus",
    actions:'<span class="coming-link">The dedicated GORM public surface is the next product milestone.</span>'
  }
};

const byId=id=>document.getElementById(id);
function selectProject(key,button){
  const project=projects[key];
  document.querySelectorAll("[data-project]").forEach(item=>{
    const active=item===button;
    item.classList.toggle("active",active);
    item.setAttribute("aria-selected",String(active));
  });

  const panel=byId("focus-panel");
  panel.className="focus-panel "+project.className;
  byId("focus-index").textContent=project.index;
  byId("focus-status").textContent=project.status;
  byId("focus-symbol").textContent=project.symbol;
  byId("focus-category").textContent=project.category;
  byId("focus-name").textContent=project.name;
  byId("focus-promise").textContent=project.promise;
  byId("focus-description").textContent=project.description;
  byId("focus-boundary").textContent=project.boundary;
  byId("focus-actions").innerHTML=project.actions;
}

document.querySelectorAll("[data-project]").forEach(button=>{
  button.addEventListener("click",()=>selectProject(button.dataset.project,button));
});

if("IntersectionObserver" in window){
  const observer=new IntersectionObserver(entries=>entries.forEach(entry=>{
    if(entry.isIntersecting){
      entry.target.classList.add("visible");
      observer.unobserve(entry.target);
    }
  }),{threshold:.08});
  document.querySelectorAll(".reveal").forEach(element=>observer.observe(element));
}else{
  document.querySelectorAll(".reveal").forEach(element=>element.classList.add("visible"));
}
