open System.Windows.Forms
open System.Collections.Generic
open System.Drawing
open System.Text.RegularExpressions
let f= new Form(TopMost=true)
f.Show()



//evento aggiornamento Clipboard
type CLipUpdate()=
    let t=new Timer(Interval=1)
    let mutable oldMatstr=Clipboard.GetText()
    let mutable oldMatFD=Clipboard.GetFileDropList()
    let evt=new Event<System.EventArgs>()

    do t.Tick.Add(fun e->
            let currMatstr=Clipboard.GetText()
            let currMatFD=Clipboard.GetFileDropList()
    
            if currMatstr=oldMatstr || currMatFD=oldMatFD then
                ()
             else
                evt.Trigger(new System.EventArgs())
        
                oldMatstr<-currMatstr
    
        )
    do t.Start()
    member this.CUpdate=evt.Publish
    
//========================================================================
type appunto()=
    let mutable str=""
    let mutable tipo= -1
    let mutable pathIm=""
    let mutable location=PointF()
    let fnt=new Font(FontFamily.GenericSansSerif,11.f)
    (*
        tipi
            0->text
            1->img
            2->filepath group
    *)
    let paint (g:Graphics)=
        match tipo with 
            |0->g.DrawString(str,fnt,Brushes.Black,location)
            |1->()
            |2->g.DrawString(str,fnt,Brushes.Black,location)
            |_->()

    member this.Paint=paint
    member this.STR
        with get()=str
        and set(v)=str<-v
    member this.Location
        with get()=location
        and set(v)=location<-v

    member this.TIPO
        with get()=tipo
        and set(v)=tipo<-v
    member this.PathIm
        with get()=pathIm
        and set(v)=pathIm<-v
//===========================================================================
type clipH()=
    let mutable applist=new List<appunto>()
    let mutable maxCapacity=10
    let mutable currCapacity=0
    
    let Updater=new CLipUpdate()
    let clear ()=
        applist.Clear()
        currCapacity<-0

    do Updater.CUpdate.Add(fun _->
        if Clipboard.ContainsText() then
            let tmpSTr=Clipboard.GetText()
            let alt=tmpSTr.Split('\n').Length
            let appstrT=new appunto(STR=tmpSTr,Location=PointF(10.f , single currCapacity*15.f),TIPO=0)
            applist.Add(appstrT)
            currCapacity<-currCapacity+alt
        
        if Clipboard.ContainsFileDropList() then
            let tmpFD=Clipboard.GetFileDropList()
            let a=tmpFD.GetEnumerator()
            while(a.MoveNext()) do
                let appFDT=new appunto(STR=a.Current,Location=PointF(10.f , single (currCapacity)*15.f),TIPO=2)
                applist.Add(appFDT)
                currCapacity<-currCapacity+1
        )
    
    let paint (g:Graphics)=
        applist |> Seq.iter (fun b->
            b.Paint g
            )
    member this.Paint = paint
    member this.Clear=clear

//===========================================================================

type ed() as this=
    inherit UserControl()
    do this.SetStyle(ControlStyles.AllPaintingInWmPaint 
                     ||| ControlStyles.OptimizedDoubleBuffer, true)

    let b=new Button(Text="clear",Left=f.Width-100,Top=20,Width=100,Height=20)
    do this.Controls.Add(b)
    
    let aaa= new clipH()
    do b.Click.Add(fun _->aaa.Clear())

    let t= new Timer(Interval=1)
    do t.Tick.Add(fun _->this.Invalidate())
    do t.Start()

    override this.OnPaint e=
              aaa.Paint e.Graphics

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
let n=new ed(Dock=DockStyle.Fill)
f.Controls.Add(n)
f.Invalidate()
n.Focus()
