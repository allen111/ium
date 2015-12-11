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
        



    
//====================================================================================================================================
//------------------------------------------------------------------------------------------------------------------------------------

type btn()=
    let mutable rect=Rectangle()
    let mutable region= new Region(rect)
    let mutable str="def"
    let fnt=new Font(FontFamily.GenericSansSerif,13.f)
    let clickDown=new Event<System.EventArgs>()
    let check(p:Point)=
        if region.IsVisible(p) then
            clickDown.Trigger(null)        
            true
        else
            false
    let paint (g:Graphics)=
        g.FillRectangle(Brushes.Blue,rect)
        g.DrawString(str,fnt,Brushes.Black,PointF(single rect.Left, single rect.Top))

    member this.Click=clickDown.Publish
    member this.Rectangle
        with get()=rect
        and set(v)=rect<-v;region<- new Region(rect)
    member this.String
        with get()=str
        and set(v)=str<-v   
    member this.Paint=paint
    member this.Check=check


//************************************************************************************************************************************
type appunto()=
    let mutable str=""
    let mutable tipo= -1
    let mutable img=null
    let mutable path=""
    let mutable location=PointF()
    let mutable altezza=1
    let fnt=new Font(FontFamily.GenericSansSerif,11.f)
    let mutable sizedString=str
    let mutable selected=true
    let mutable bru=Brushes.Black
    let sizing ()=
        if str.Length>30 then
            sizedString<-str.Substring(0,30)+"..."
        else
            sizedString<-str
        altezza<-sizedString.Split('\n').Length + 1
    (*
        tipi
            0->text
            1->img file
            2->filepath group
            3->image(bitmap) screenshot
    *)

    let paint (g:Graphics)(p:PointF)=
        if selected then
                    bru<-Brushes.Red
                else
                    bru<-Brushes.Black
        match tipo with 
            |0->g.DrawString(sizedString,fnt,bru,p)
            |1->()
            |2->g.DrawString(str,fnt,bru,p)
            |3->
                if selected then
                    g.DrawRectangle(Pens.Red,Rectangle(int p.X-1, int p.Y-1,121,101))
                g.DrawImage(img,Rectangle(int p.X, int p.Y,120,100))
            |_->()

    member this.Paint=paint
    member this.STR
        with get()=str
        and set(v)=str<-v;sizing()
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
    member this.Selected
        with get()=selected
        and set(v)=selected<-v
    member this.Img
        with get()=img
        and set(v)=img<-v
//===========================================================================
type clipH()=
    let mutable applist=new List<appunto>()
    let mutable maxCapacity=10
    let mutable currCapacity=0
    let mutable currSelected=0
    
    let evtUpd=new Event<System.EventArgs>()
    let Updater=new ClipboardAux()
    let clear ()=
        applist.Clear()
        currCapacity<-0

    let Update ()=
        if Clipboard.ContainsText() then
                let tmpSTr=Clipboard.GetText()
                let alt=tmpSTr.Split('\n').Length
                let appstrT=new appunto(STR=tmpSTr,Location=PointF(10.f , single currCapacity*15.f),TIPO=0)
                
                applist |> Seq.iter (fun b-> b.Selected<-false)
                applist.Add(appstrT)
                evtUpd.Trigger(null)
                currSelected<-1
                currCapacity<-currCapacity+alt
        else
         if Clipboard.ContainsFileDropList() then
            
            let tmpFD=Clipboard.GetFileDropList()
            let a=tmpFD.GetEnumerator()
            applist |> Seq.iter (fun b-> b.Selected<-false)
            let mutable h=1
            while(a.MoveNext()) do
                let tmpBs=a.Current.Split('\\')
                let tmpb=tmpBs.[tmpBs.Length-1]
                let appFDT=new appunto(STR=tmpb,Path=a.Current,Location=PointF(10.f , single (currCapacity)*15.f),TIPO=2,Altezza=1,Selected=true)
                applist.Add(appFDT)
                evtUpd.Trigger(null)
                currSelected<-1
                currCapacity<-currCapacity+1
                h<-h+1
         else
            if Clipboard.ContainsImage() then
                let tmpIMG=Clipboard.GetImage()
                applist |> Seq.iter (fun b-> b.Selected<-false)
                let appIMG=new appunto(Img=tmpIMG,Altezza=8,Selected=true,TIPO=3)
                applist.Add(appIMG)
                evtUpd.Trigger(null)
                currSelected<-1
                currCapacity<-currCapacity+1
                

    let mutable x= Updater.ClipboardChanged.Subscribe (fun e->
                Update()
                )
    let shiftdown () =
        let mutable tmpA=None
        if currSelected>0 then
            if applist.Count>currSelected then
                x.Dispose()
                applist |> Seq.iter (fun b-> b.Selected<-false)
                let tmpSel=applist.[applist.Count-currSelected-1]
                applist.[applist.Count-currSelected-1].Selected<-true
                tmpA<-Some tmpSel
                Clipboard.Clear()
                match tmpSel.TIPO with
                    |0->Clipboard.SetText(tmpSel.STR)
                    |3->Clipboard.SetImage(tmpSel.Img)
                    |_->()
                x<- Updater.ClipboardChanged.Subscribe (fun e->
                    Update()
                )
                currSelected<-currSelected+1
        tmpA


    let shiftup () =
        let mutable tmpA=None
        if currSelected>1  then
            x.Dispose()
            applist |> Seq.iter (fun b-> b.Selected<-false)
            let tmpSel=applist.[applist.Count-currSelected+1]
            applist.[applist.Count-currSelected+1].Selected<-true
            tmpA<-Some tmpSel

            Clipboard.Clear()
            match tmpSel.TIPO with
                    |0->Clipboard.SetText(tmpSel.STR)
                    |3->Clipboard.SetImage(tmpSel.Img)
                    |_->()
            x<- Updater.ClipboardChanged.Subscribe (fun e->
                    Update()
                )
            currSelected<-currSelected-1
        tmpA
            

    let paint (g:Graphics)=
        let len=applist.Count 
        let mutable pt=PointF(10.f,10.f)
        for b in len-1 .. -1 .. 0 do
            applist.[b].Paint g pt
            applist.[b].Location<-pt
            pt<- PointF(10.f,pt.Y+single(applist.[b].Altezza)*15.f)
    


    member this.Paint = paint
    member this.Clear=clear
    member this.ShiftDown=shiftdown
    member this.ShiftUp=shiftup
    member this.Aux=Updater
    member this.UpdateEvt=evtUpd.Publish
//===========================================================================

type ed() as this=
    inherit UserControl()
    do this.SetStyle(ControlStyles.AllPaintingInWmPaint 
                     ||| ControlStyles.OptimizedDoubleBuffer, true)
    let mutable w2v = new Drawing2D.Matrix()
    let mutable v2w = new Drawing2D.Matrix()
    let aaa= new clipH()

    let transformP (m:Drawing2D.Matrix) (p:Point) =
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0]
  
    let translateW (tx, ty) =
        w2v.Translate(tx, ty)
        v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)

    let transformP (m:Drawing2D.Matrix) (p:PointF) =
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0]
    
    
    do aaa.UpdateEvt.Add(fun _->w2v<-new Drawing2D.Matrix();v2w<-new Drawing2D.Matrix()) 
    
    let scrool (app:appunto)=
        let tmpP=transformP w2v app.Location
        
        if app.TIPO=3 && tmpP.Y+100.f> single this.Height then
            let h=tmpP.Y+90.f- single this.Height
            translateW(0.f,-h)
        if app.TIPO=3 && tmpP.Y<20.f then
            let h=Math.Abs(tmpP.Y)+10.f
            translateW(0.f,h)
        if app.TIPO=0 &&  tmpP.Y+15.f> single this.Height then
            let h= tmpP.Y+single (app.Altezza*15) - single this.Height
            translateW(0.f,-h)
        if app.TIPO=0 && tmpP.Y<15.f then
            let h=Math.Abs(tmpP.Y)+10.f
            translateW(0.f,h)

  
    
//    let b=new Button(Text="clear",Left=f.Width-100,Top=20,Width=100,Height=20)
//    do this.Controls.Add(b)
//  
    //do b.Click.Add(fun _->aaa.Clear();Clipboard.Clear();w2v<- new Drawing2D.Matrix();v2w <- new Drawing2D.Matrix())

    let t= new Timer(Interval=1)
    do t.Tick.Add(fun _->this.Invalidate())
    do t.Start()
    
    override this.OnMouseWheel  e=
        //scroll base 
        if e.Delta>0 then
            translateW(0.f,15.f)
        else
            translateW(0.f,-15.f)

    

    override this.OnKeyDown e=
        
        
        match e.KeyCode with
            |Keys.S->
                let x=(aaa.ShiftDown())
                if x.IsSome then
                    scrool x.Value    
            |Keys.W->
                let x=(aaa.ShiftUp())
                if x.IsSome then
                    scrool x.Value    
            |_->()

    override this.OnPaint e=
              
              e.Graphics.Transform<-w2v
              aaa.Paint e.Graphics
              
    
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
let n=new ed(Dock=DockStyle.Fill)
f.Controls.Add(n)
f.TopMost<-true
f.Invalidate()
n.Focus()






