open  System;
open  System.Runtime.InteropServices;

open System.Windows.Forms
open System.Collections.Generic
open System.Drawing
open System.Text.RegularExpressions

module climod =         
//  module private clim = 
    [<DllImport( "user32.dll")>]
    extern int SetClipboardViewer(int hWndNewViewer)

    [<DllImport("User32.dll", CharSet = CharSet.Auto)>]
    extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext)

    [<DllImport("user32.dll", CharSet = CharSet.Auto)>]
    extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam)

  


let f= new Form(TopMost=true)
f.Show()


type ClipboardChangedEventArgs ()=
     let a=2
     //nel caso mettici qualcosa
//nuovo aggiornamento migliorato e sticazzi
type ClipboardAux()as this=
    inherit Form()
    let ClipboardChangedEvent=new Event<ClipboardChangedEventArgs>()
    let mutable nextClipboardViewer =IntPtr(1)

    do 
        nextClipboardViewer<-IntPtr( climod.SetClipboardViewer(int this.Handle))
    
    [<System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")>]
    override this.WndProc (m : Message byref) =
        let WM_DRAWCLIPBOARD = 0x308;
        let WM_CHANGECBCHAIN = 0x030D;
        
        match m.Msg with
         |wm when wm=WM_DRAWCLIPBOARD->
            //fai quello che vuoi clipboard changed
            
            ClipboardChangedEvent.Trigger(new ClipboardChangedEventArgs())
            let tmo=climod.SendMessage(nextClipboardViewer,m.Msg,m.WParam,m.LParam)
            ()
         |wm when wm=WM_CHANGECBCHAIN->
            if m.WParam=nextClipboardViewer then
                nextClipboardViewer<-m.LParam
            else
                let tmp=climod.SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam)
                ()
         |_->base.WndProc(&m)


    override this.Dispose e=
        let t=climod.ChangeClipboardChain(this.Handle,nextClipboardViewer)
        ()

    member this.ClipboardChanged=ClipboardChangedEvent.Publish        



    
//========================================================================
type appunto()=
    let mutable str=""
    let mutable tipo= -1
    let mutable path=""
    let mutable location=PointF()
    let mutable altezza=1
    let fnt=new Font(FontFamily.GenericSansSerif,11.f)
    (*
        tipi
            0->text
            1->img file
            2->filepath group
            3->image(bitmap)
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
    member this.Altezza
        with get()=altezza
        and set(v)=altezza<-v
    member this.TIPO
        with get()=tipo
        and set(v)=tipo<-v
    member this.Path
        with get()=path
        and set(v)=path<-v
//===========================================================================
type clipH()=
    let mutable applist=new List<appunto>()
    let mutable maxCapacity=10
    let mutable currCapacity=0
    
    let Updater=new ClipboardAux()
    let clear ()=
        applist.Clear()
        currCapacity<-0

    do Updater.ClipboardChanged.Add(fun _->
        if Clipboard.ContainsText() then
            let tmpSTr=Clipboard.GetText()
            let alt=tmpSTr.Split('\n').Length
            let appstrT=new appunto(STR=tmpSTr,Location=PointF(10.f , single currCapacity*15.f),TIPO=0,Altezza=alt)
            applist.Add(appstrT)
            currCapacity<-currCapacity+alt
        else
         if Clipboard.ContainsFileDropList() then
            let tmpFD=Clipboard.GetFileDropList()
            let a=tmpFD.GetEnumerator()
            while(a.MoveNext()) do
                let tmpBs=a.Current.Split('\\')
                let tmpb=tmpBs.[tmpBs.Length-1]
                let appFDT=new appunto(STR=tmpb,Path=a.Current,Location=PointF(10.f , single (currCapacity)*15.f),TIPO=2)
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
    do b.Click.Add(fun _->aaa.Clear();Clipboard.Clear())

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
