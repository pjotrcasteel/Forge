const sections=["state","plan","order","stale","replan"];
const labels=[...document.querySelectorAll(".story-progress span")];

if("IntersectionObserver" in window){
  const observer=new IntersectionObserver(entries=>{
    const visible=entries
      .filter(entry=>entry.isIntersecting)
      .sort((a,b)=>b.intersectionRatio-a.intersectionRatio)[0];

    if(!visible){return;}

    const index=sections.indexOf(visible.target.id);
    labels.forEach((label,labelIndex)=>label.classList.toggle("active",labelIndex===index));
  },{rootMargin:"-28% 0px -55% 0px",threshold:[0,.2,.5]});

  sections.forEach(id=>{
    const section=document.getElementById(id);
    if(section){observer.observe(section);}
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
