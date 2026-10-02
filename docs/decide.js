const sections=["space","propose","decide","shadow","evidence"];
const labels=[...document.querySelectorAll(".story-progress span")];

async function copyText(value,button,idle){
  const target=button.querySelector(".copy-label")||button;
  const fallback=idle||target.textContent;

  try{
    await navigator.clipboard.writeText(value);
    target.textContent="Copied";
  }catch{
    target.textContent="Select";
  }

  setTimeout(()=>target.textContent=fallback,1300);
}

document.querySelectorAll("[data-copy]").forEach(button=>
  button.addEventListener("click",()=>copyText(button.dataset.copy,button,"Copy")));

document.querySelectorAll('a[href*="/tree/feature/forge-decide/"]').forEach(link=>{
  link.href=link.href.replace("/tree/feature/forge-decide/","/tree/main/");
});

if("IntersectionObserver" in window){
  const progressObserver=new IntersectionObserver(entries=>{
    const visible=entries
      .filter(entry=>entry.isIntersecting)
      .sort((a,b)=>b.intersectionRatio-a.intersectionRatio)[0];

    if(!visible){return;}

    const index=sections.indexOf(visible.target.id);
    labels.forEach((label,labelIndex)=>label.classList.toggle("active",labelIndex===index));
  },{rootMargin:"-28% 0px -55% 0px",threshold:[0,.2,.5]});

  sections.forEach(id=>{
    const section=document.getElementById(id);
    if(section){progressObserver.observe(section);}
  });

  const revealObserver=new IntersectionObserver(entries=>entries.forEach(entry=>{
    if(entry.isIntersecting){
      entry.target.classList.add("visible");
      revealObserver.unobserve(entry.target);
    }
  }),{threshold:.08});

  document.querySelectorAll(".reveal").forEach(element=>revealObserver.observe(element));
}else{
  document.querySelectorAll(".reveal").forEach(element=>element.classList.add("visible"));
  labels[0]?.classList.add("active");
}