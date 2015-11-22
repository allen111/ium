open System.Windows.Forms
open System.Drawing
open System.Collections.Generic


let f= new Form(TopMost=true)
f.Show()



type Icon()=
    let mutable img=Image.FromFile(@"C:\Users\allen\Pictures\iconWIP\Def.png")
    let mutable location=Point()
    let mutable reg= new Region(Rectangle(location.X-5,location.Y-5,60,60))
    let mutable hovB=false
    let mutable bru= Brushes.Gray
    let mutable dragging=false
    let mutable offset=Point()

    let md (e:Point)=
       if hovB then
         offset<-Point(e.X-location.X,e.Y-location.Y)
         dragging<-true
         bru<-Brushes.Red
       dragging

    let trascina (e:Point) =
        if dragging  then
            location<-Point(e.X-offset.X,e.Y-offset.Y)
            reg<-new Region(Rectangle(location.X-5,location.Y-5,60,60))
        
    let hov (e:Point)=
      if not dragging then
        if reg.IsVisible(e) then
            hovB<-true
            bru<-Brushes.LightGray
        else
            hovB<-false
            bru<-Brushes.Gray
      hovB

    let mu (b:bool) (p:Point)=
      let mutable tmp=false
      if hovB && not b then
        printfn "urray"
        tmp<-true
      if b then
        if p.X <> -1 && p.Y <> -1  && dragging then
            location<-Point(p.X+7,p.Y+7)
            reg<-new Region(Rectangle(location.X-5,location.Y-5,60,60))
        dragging<-false
      tmp

    let paint (e:Graphics)=
        e.FillRectangle(bru,location.X-5,location.Y-5,60,60)
        e.DrawImage(img,location.X,location.Y,50,50)

    member this.Img
        with get()=img
        and set(v)=img<-v

    member this.Drag
        with get()=dragging
        and set(v)=dragging<-v

    member this.Location
        with get()=location
        and set(v)=location<-v; reg<-new Region(Rectangle(location.X-5,location.Y-5,60,60))

    member this.Paint=paint
    member this.Hover=hov
    member this.Mu=mu
    member this.Trascina=trascina
    member this.Md=md

    //-------------------------------------------------------------------------------------------------------

type SlotBar()=
    let mutable number=5
    let mutable size=63
    let mutable location=Point()
    let mutable reg = new Region(Rectangle(location.X,location.Y,size*number,size))

    let check (e:Point) =
        if reg.IsVisible(e) then
            let num= (e.X-location.X)/size
            Point(location.X+(size*num),location.Y)

        else
            Point(-1,-1)

    let paint (g:Graphics)=
        for i in 0..number-1 do
            let s = g.Save()
            g.TranslateTransform(single(location.X+(i*size)), single(location.Y))
            g.DrawRectangle(Pens.DarkOrange,0,0,size,size)
            g.Restore(s)

   
    member this.Number
        with get()=number
        and set(v)=number<-v
    member this.Size
        with get()=size
        and set(v)=size<-v
    member this.Location
        with get()=location
        and set(v)=location<-v;reg <- new Region(Rectangle(location.X,location.Y,size*number,size))
    member this.Paint=paint
    member this.Check=check


    //-------------------------------------------------------------------------------------------------------

type Cont() as this=
    inherit UserControl()
    do this.SetStyle(ControlStyles.AllPaintingInWmPaint 
                     ||| ControlStyles.OptimizedDoubleBuffer, true)

    let mutable MODactive=false
    
    let img1=Image.FromFile(@"C:\Users\allen\Pictures\iconWIP\Minecraft.png")
    let slots=new SlotBar(Location=Point(20,200))
    
    let mutable icons= new List<Icon>()
    do
                icons.Add(new Icon( Location=Point(6,6)))
                icons.Add(new Icon(Img=img1, Location=Point(67,6)))
                icons.Add(new Icon(Img=img1, Location=Point(128,6)))
                icons.Add(new Icon(Img=img1, Location=Point(189,6)))
                

    override this.OnMouseDown e=
        let mutable tmp=false
        let mutable tmpI= -1
        icons|> Seq.iteri (fun i b->
           if MODactive then
              if not tmp then
                tmp<-b.Md e.Location
                if tmp then tmpI<-i 
                    
            )
        if tmpI>=0 then
           let x=icons.[tmpI]
           icons.RemoveAt(tmpI)
           icons.Add(x)
           tmpI<- -1     

        this.Invalidate()

    override this.OnMouseMove e=
        let mutable tmp=false
       // icons|> Seq.iter (fun b->
        for b in 3 .. -1 .. 0 do
          if not tmp then
            tmp<-icons.[b].Hover e.Location
            if MODactive then
                 icons.[b].Trascina e.Location
          else
              let a=icons.[b].Hover (Point(-1,-1))
              if MODactive then
                 icons.[b].Trascina e.Location
            
        
        this.Invalidate()

    override this.OnMouseUp e =
            let mutable tmpI= -1
            let mutable tmp2=false

            icons|> Seq.iteri (fun i b->
                let tmp=slots.Check e.Location
                tmp2<-b.Mu MODactive tmp
                if tmp2 then
                    tmpI<-i
                )
            if tmpI>=0 then
                let x=icons.[tmpI]
                icons.RemoveAt(tmpI)
                icons.Add(x)
                tmpI<- -1 
            this.Invalidate()

    
    override this.OnKeyDown e=
        match e.KeyCode with
            |Keys.ShiftKey->MODactive<-true;this.Invalidate()
            |_->()

    override this.OnKeyUp e=   
        match e.KeyCode with
            |Keys.ShiftKey->
                MODactive<-false;this.Invalidate()
                icons|> Seq.iter (fun b->
                b.Drag<-false
                )
            |_->()

    override this.OnPaint e=
        let g= e.Graphics
        slots.Paint g
        if MODactive then
            g.DrawRectangle(Pens.Red,1,1,f.ClientSize.Width-2,f.ClientSize.Height-2)

        for b in 0..3 do
            icons.[b].Paint g
            
    override this.OnResize e=
        base.OnResize e
        this.Invalidate()


let a1= new Cont(Dock=DockStyle.Fill)
f.Controls.Add(a1)
//f.BackColor<-Color.Magenta
//f.TransparencyKey<-Color.Magenta

a1.Focus()
f.Invalidate()