open System.Windows.Forms
open System.Drawing
let f = new Form(Text="hello", TopMost=true,Height=720,Width=1280)

f.Show()
//transparenza done
//usa solo drawImage





type Mappa() =

  inherit UserControl()
  let scale=64
  let mutable Height1=1
  let mutable Width1=1
  let gap=64
  let mutable j,i=(640/scale)-1,360/scale
  let mutable x=j*scale
  let mutable y=i*scale
  let mutable currX,currY=x,y
  let mutable stato=0
  let mutable bru= Brushes.Red
  let  mutable loaded = false
  let mutable pic= new PictureBox()
  let mutable m2DArray= Array2D.zeroCreate<int> 5 5
  let mutable w2v=new Drawing2D.Matrix()
  let mutable v2w=new Drawing2D.Matrix()
  let imFront=Image.FromFile("C:/Users/allen/Desktop/LinkFront.png")
  let imLeft=Image.FromFile("C:/Users/allen/Desktop/LinkLeft.png")
  let imRight=Image.FromFile("C:/Users/allen/Desktop/LinkRight.png")
  let imBack=Image.FromFile("C:/Users/allen/Desktop/LinkBack.png")
  let imgTile = Image.FromFile("C:/Users/allen/Desktop/Wall.png")
  let imgFloor= Image.FromFile("C:/Users/allen/Desktop/Floor.png")
  let imgDoor =Image.FromFile("C:/Users/allen/Desktop/Door.png")
  let mutable drawingImg=imFront
  let mutable Dir=1

  let RetDir x=
    match x with
        |0->m2DArray.[i-1,j]
        |1->m2DArray.[i+1,j]
        |2->m2DArray.[i,j-1]
        |3->m2DArray.[i,j+1]
        |_-> -1

  let translateW (tx, ty) =
    w2v.Translate(tx, ty)
    v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)
  
  let transformP (m:Drawing2D.Matrix) (p:Point) =
    let a = [| Point( p.X,  p.Y) |]
    m.TransformPoints(a)
    a.[0]
 
  override this.OnLoad e =
    let arrS=System.IO.File.ReadAllLines(@"C:\Users\allen\Desktop\b1.txt")
    Height1<-System.Int32.Parse(arrS.[0])
    Width1<-System.Int32.Parse(arrS.[1])
    m2DArray<-Array2D.zeroCreate<int> Height1 Width1
    let mutable count=2
    
    for a in 0..Height1-1 do (
       for b in 0..Width1-1 do(
                m2DArray.[a,b]<-System.Int32.Parse(arrS.[count])
                count<-count+1
                                          )    
                                     )
    this.Invalidate()

//attivazione double buffering
  override  this.CreateParams
      with get()=
                 let cp = base.CreateParams
                 let a= cp.ExStyle<- 0x02000000
                 cp
//paint
  override this.OnPaint e =
    let g = e.Graphics  
    g.Transform<-w2v
    
    for a in 0..Height1-1 do (
        for b in 0..Width1-1 do(
          let pt= Point((b*scale),(a*scale))

          match m2DArray.[a,b] with
              |1-> g.DrawImage(imgTile,Rectangle(pt.X,pt.Y,scale,scale))
              |2-> g.DrawImage(imgDoor,Rectangle(pt.X,pt.Y,scale,scale))
              |3-> g.DrawImage(imgDoor,Rectangle(pt.X,pt.Y,scale,scale))
              |0-> g.DrawImage(imgFloor,Rectangle(pt.X,pt.Y,scale,scale))
              |_->()
                     )    
                   )
    g.DrawImage(drawingImg,Rectangle(x,y,scale,scale))
    base.OnPaint(e)



  override this.OnKeyDown e =
   currX<-x
   currY<-y
   match e.KeyCode with
    | Keys.E->printfn "use"
              if RetDir Dir =2 then
                                        match Dir with
                                            |0->m2DArray.[i-1,j]<-3
                                            |1->m2DArray.[i+1,j]<-3
                                            |2->m2DArray.[i,j-1]<-3
                                            |3->m2DArray.[i,j+1]<-3
                                            |_->()
    | Keys.W-> 
        drawingImg<-imBack
        Dir<-0
        if m2DArray.[i-1,j]=0 || m2DArray.[i-1,j]=3 then 
                                                            y<-y-gap;
                                                            i<-i-1;
                                                            translateW(0.f,single(scale))
                                   
    | Keys.S -> 
        Dir<-1
        drawingImg<-imFront
        if m2DArray.[i+1,j]=0 || m2DArray.[i+1,j]=3 then
                                                          i<-i+1
                                                          y<-y+gap
                                                          translateW(0.f,-single(scale))
    | Keys.A -> 
        Dir<-2
        drawingImg<-imLeft
        if m2DArray.[i,j-1]=0 || m2DArray.[i,j-1]=3 then
                                                         x<-x-gap 
                                                         j<-j-1
                                                         translateW(single(scale),0.f)
    | Keys.D ->
        Dir<-3
        drawingImg<-imRight
        if m2DArray.[i,j+1]=0 || m2DArray.[i,j+1]=3 then
                                                         x<-x+gap;
                                                         j<-j+1 ;
                                                         translateW(-single(scale),0.f)
    | _ ->()


   this.Invalidate()
   



let e= new Mappa(Dock=DockStyle.Fill)

f.Controls.Add(e)
e.Focus()
