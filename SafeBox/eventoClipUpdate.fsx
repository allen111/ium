open System.Windows.Forms

open System.Drawing


//evento update clipboard
let t=new Timer(Interval=1)
let mutable oldMatstr=Clipboard.GetText()
let mutable oldMatFD=Clipboard.GetFileDropList()
let evt=new Event<System.EventArgs>()
evt.Publish.Add(fun _->printfn "changed")
t.Tick.Add(fun e->
    let currMatstr=Clipboard.GetText()
    let currMatFD=Clipboard.GetFileDropList()
    
    if currMatstr=oldMatstr || currMatFD=oldMatFD then
        ()
     else
        evt.Trigger(new System.EventArgs())
        
        oldMatstr<-currMatstr
    
    )

t.Start()
t.Stop()