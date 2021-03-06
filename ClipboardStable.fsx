#load "imageComb.fsx"
open  System;
open  System.Runtime.InteropServices;

open System.Windows.Forms
open System.Collections.Generic
open System.Drawing
open System.Text.RegularExpressions
open ImageComb

module climod =         
//  module private clim = 
    [<DllImport( "user32.dll")>]
    extern int SetClipboardViewer(int hWndNewViewer)

    [<DllImport("User32.dll", CharSet = CharSet.Auto)>]
    extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext)

    [<DllImport("user32.dll", CharSet = CharSet.Auto)>]
    extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam)

  


let f= new Form(Text="ClipBoard Manager",TopMost=true,Width=300,Height=500)
f.Show()


type ClipboardChangedEventArgs ()=
     let a=2
     //nel caso mettici qualcosa

//nuovo aggiornamento migliorato
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
//i bottoni in basso
type btn()=
    let mutable rect=Rectangle()
    let mutable region= new Region(rect)
    let mutable str="def"
    let fnt=new Font(FontFamily.GenericSansSerif,13.f)
    let clickDown=new Event<System.EventArgs>()
    let check(p:Point)=
        if region.IsVisible(p) then
            clickDown.Trigger(null)        
            
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
    member this.ClickT=clickDown.Trigger


//************************************************************************************************************************************
//contenitore dei bottoni (comprende anche le scorciatoie da tastiera)
type ButtonContainer() as this=
    inherit UserControl()
    let mutable buttons= new ResizeArray<btn>()
    let bt1=new btn(Rectangle=Rectangle(0,0,50,30),String="giu")
    let bt2=new btn(Rectangle=Rectangle(51,0,50,30),String="su")
    let bt3=new btn(Rectangle=Rectangle(102,0,50,30),String="clear")
    let bt4=new btn(Rectangle=Rectangle(153,0,50,30),String="edit")
    let bt5=new btn(Rectangle=Rectangle(204,0,50,30),String="comb")
    do buttons.Add(bt1);buttons.Add(bt2);buttons.Add(bt3);buttons.Add(bt4);buttons.Add(bt5)
    do bt1.Click.Add(fun _-> (printfn"1" ))
    do bt2.Click.Add(fun _-> (printfn"2" ))
    do bt3.Click.Add(fun _-> (printfn"3" ))
    do bt4.Click.Add(fun _-> (printfn"4" ))
    do bt5.Click.Add(fun _-> (printfn"5" ))
    

    override this.OnMouseUp e=
        buttons |> Seq.iter (fun b->
            b.Check e.Location
            )

    override this.OnPaint e=
        buttons |> Seq.iter (fun b->
            b.Paint e.Graphics 
            )

    override this.OnKeyDown e=
        match e.KeyCode with
            |Keys.S->bt1.ClickT(null)
            |Keys.W->bt2.ClickT(null)
            |_->()

    member this.Buttons
        with get()=buttons
        and set(v)=buttons<-v

//3333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333
//tipo di un singolo appunto
type appunto()=
    
    let mutable str=""
    let mutable tipo= -1
    let mutable img=null
    let mutable path=null
    let mutable strs=new ResizeArray<String>()
    let mutable location=PointF()
    let mutable altezza=1
    let fnt=new Font(FontFamily.GenericSansSerif,11.f)
    let mutable sizedString=str
    let mutable selected=true
    let mutable bru=Brushes.Black
    let mutable reg=new Region()
    let regRes(pf:PointF)=
        match tipo with
            |0->reg<-new Region(Rectangle(int location.X-3,int location.Y-5,f.ClientSize.Width-10,(altezza*15)-5))
            |2->reg<-new Region(Rectangle(int location.X-3,int location.Y-5,f.ClientSize.Width-10,(altezza*15)-5))
            |3->reg<-new Region(Rectangle(int location.X-3,int location.Y-5,f.ClientSize.Width-10,112))
            |_->()

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
            |0->g.FillRegion(Brushes.Aqua,reg);g.DrawString(sizedString,fnt,bru,location);
            |1->()
            |2->
                g.FillRegion(Brushes.Aqua,reg);
                strs|>Seq.iteri (fun i b->  g.DrawString(b,fnt,bru,PointF(p.X,p.Y+single(i*15))))
            |3->
                g.FillRegion(Brushes.Aqua,reg)
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
        and set(v)=location<-v;regRes(v)
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
    member this.STRS
        with get()=strs
        and set(v)=strs<-v;altezza<-v.Count+1
    member this.Reg
        with get()=reg
//===========================================================================
//contenitore degli appunti con funzioni degli appunti 
type clipH() =
    let mutable applist=new List<appunto>()
    let mutable maxCapacity=10
    let mutable currCapacity=0
    let mutable currSelected=0
    
    let evtUpd=new Event<System.EventArgs>()
    let Updater=new ClipboardAux()
    let clear ()=
        applist.Clear()
        Clipboard.Clear()
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
            
            let strarr=new ResizeArray<String>()
            while(a.MoveNext()) do
                let tmpBs=a.Current.Split('\\')
                let tmpb=tmpBs.[tmpBs.Length-1]
                strarr.Add(tmpb)     
                
                
            let appFDT=new appunto(STRS=strarr,Path=tmpFD,Location=PointF(10.f , single (currCapacity)*15.f),TIPO=2,Selected=true)
            printfn"%A" tmpFD
            applist.Add(appFDT)
            evtUpd.Trigger(null)
            currSelected<-1
            currCapacity<-currCapacity+1
            
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
    

    let getTextT()=
        let p=Cursor.Position
        let mutable ret="hello"
        applist|>Seq.iter (fun b->
            
            if b.Reg.IsVisible(p) && b.TIPO=0 then
                ret<-b.STR
          
        )
        ret

    let getImgs ()=
        let imgs=new ResizeArray<Image>()
        applist|>Seq.iter(fun b->
            if b.TIPO=3 then
                imgs.Add(b.Img)
            )
        imgs


    let selecting (p:Point)=
        applist|>Seq.iteri (fun i b->
            if b.Reg.IsVisible(p) then
                x.Dispose()
                applist |> Seq.iter (fun b-> b.Selected<-false)
                b.Selected<-true
                match b.TIPO with
                    |0->Clipboard.SetText(b.STR)
                    |2->
                          Clipboard.SetFileDropList(b.Path);printfn"%A" b.Path

                    |3->Clipboard.SetImage(b.Img)
                    |_->()
                x<- Updater.ClipboardChanged.Subscribe (fun e->
                    Update()
                )
                currSelected<-applist.Count-i
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
                
                match tmpSel.TIPO with
                    |0->Clipboard.SetText(tmpSel.STR)
                    |2-> Clipboard.SetFileDropList(tmpSel.Path)
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

            
            match tmpSel.TIPO with
                    |0->Clipboard.SetText(tmpSel.STR)
                    |2->Clipboard.SetFileDropList(tmpSel.Path)
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
            applist.[b].Location<-pt
            applist.[b].Paint g pt
            pt<- PointF(10.f,pt.Y+single(applist.[b].Altezza)*15.f)
    


    member this.Paint = paint
    member this.Clear=clear
    member this.ShiftDown=shiftdown
    member this.ShiftUp=shiftup
    member this.Aux=Updater
    member this.UpdateEvt=evtUpd.Publish
    member this.Selecting=selecting
    member this.GetImgs=getImgs
    member this.GetText=getTextT
//================================================================================================================================
//clipboard manager type
type ed() as this=
    inherit UserControl()

    do this.SetStyle(ControlStyles.AllPaintingInWmPaint 
                     ||| ControlStyles.OptimizedDoubleBuffer, true)


    let mutable w2v = new Drawing2D.Matrix()
    let mutable v2w = new Drawing2D.Matrix()
    let  aaa= new clipH()

    let transformPt (m:Drawing2D.Matrix) (p:Point) =
        let a = [| Point( p.X,  p.Y) |]
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
    let n1=new ButtonContainer(Dock=DockStyle.Bottom,AutoSize=false,Height=30,BackColor=Color.Cyan)
    do this.Controls.Add(n1)
    
    




    
    let scrool (app:appunto)=
    //sposta la view per visualizzare l'appunto selezionato
        let tmpP=transformP w2v app.Location
        
        if app.TIPO=3 && tmpP.Y+100.f> single this.Height then
            let h=tmpP.Y+90.f- single this.Height
            translateW(0.f,-h)
        if app.TIPO=3 && tmpP.Y<20.f then
            let h=Math.Abs(tmpP.Y)+10.f
            translateW(0.f,h)
        if app.TIPO=0 &&  tmpP.Y+15.f> (single this.Height) - 20.f then
            let h= tmpP.Y + 20.f + single (app.Altezza*15) - single this.Height
            translateW(0.f,-h)
        if app.TIPO=0 && tmpP.Y<15.f then
            let h=Math.Abs(tmpP.Y)+10.f
            translateW(0.f,h)
        if app.TIPO=2 &&  tmpP.Y+15.f> (single this.Height) - 20.f then
            let h= tmpP.Y + 20.f + single (app.Altezza*15) - single this.Height
            translateW(0.f,-h)
        if app.TIPO=2 && tmpP.Y<15.f then
            let h=Math.Abs(tmpP.Y)+10.f
            translateW(0.f,h)

  
    do n1.Buttons.[0].Click.Add(fun b-> 
        let x=(aaa.ShiftDown())
        if x.IsSome then
          scrool x.Value
        
        )
    do n1.Buttons.[1].Click.Add(fun b-> 
        let x=(aaa.ShiftUp())
        if x.IsSome then
           scrool x.Value 
        
        )

    do n1.Buttons.[2].Click.Add(fun b->
        aaa.Clear()
        
        w2v<- new Drawing2D.Matrix()
        v2w <- new Drawing2D.Matrix()
        )

    do n1.Buttons.[3].Click.Add(fun b->
        if Clipboard.ContainsText() then    
            let mutable string=Clipboard.GetText()
            let mutable h=string.Split('\n').Length+1
            
            let f2= new Form(Text="EditBox",TopMost=true)
            let editT=new TextBox(Text=string,Dock=DockStyle.Top,Multiline=true)
            if (h*editT.Font.Height)<f2.Height-100 then
                editT.Height<-h*editT.Font.Height
            else 
                editT.Height<-f2.Height-100
            //da aggiungere lo scrool se e' troppo testo
            let btalpha=new Button(Text="Done",Dock=DockStyle.Top)
            f2.Controls.Add(btalpha)
            f2.Controls.Add(editT)
            f2.Show()
            btalpha.Click.Add(fun e->editT.SelectAll();editT.Copy();f2.Close())//unico modo con SetText lo fa 2 volte booh
        )
    do n1.Buttons.[4].Click.Add(fun b->
        let mutable imgs=aaa.GetImgs()

        
        if imgs.Count<>0 then
            let mutable img1=Clipboard.GetImage()
            let f3= new Form(Text="imgcomb",TopMost=true,Size=Size(500,400))
            let cmb= new ImageCombinator(Dock=DockStyle.Fill)
            f3.Controls.Add(cmb)
            imgs.Reverse()
            cmb.AddImages(imgs)
            f3.Show()  
        
        )


    let t= new Timer(Interval=1)
    do t.Tick.Add(fun _->this.Invalidate())
    do t.Start()
   
    override this.OnMouseWheel  e=
        //scroll base
        if e.Delta>0 then
            translateW(0.f,15.f)
        else
            translateW(0.f,-15.f)

    

   

    override this.OnMouseUp e=
        e.Location |>transformPt v2w |> aaa.Selecting
        

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

              
    
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
let n=new ed(Dock=DockStyle.Fill)
f.Controls.Add(n)
f.TopMost<-true
f.Invalidate()
n.Focus()




