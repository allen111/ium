open System.Windows.Forms
open System.Drawing
let f = new Form(Text="hello", TopMost=true,Height=720,Width=1280)

f.Show()
//questo va Bitmap Implementate con successo 
//metto che si gira anche se c'e' un muro fatto





type Mappa() =

  inherit UserControl()
  let scale=64
  let mutable Height1=1
  let mutable Width1=1
  let gap=0
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
  let mutable bmpFront= new Bitmap(imFront)
  let imLeft=Image.FromFile("C:/Users/allen/Desktop/LinkLeft.png")
  let mutable bmpLeft= new Bitmap(imLeft)
  let imRight=Image.FromFile("C:/Users/allen/Desktop/LinkRight.png")
  let mutable bmpRight= new Bitmap(imRight)
  let imBack=Image.FromFile("C:/Users/allen/Desktop/LinkBack.png")
  let mutable bmpBack= new Bitmap(imBack)



  let translateW (tx, ty) =
    w2v.Translate(tx, ty)
    v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)
  
  let transformP (m:Drawing2D.Matrix) (p:Point) =
    let a = [| Point( p.X,  p.Y) |]
    m.TransformPoints(a)
    a.[0]
 
  override this.OnLoad e =
    pic.Image<-bmpFront
    pic.Size<-Size(scale,scale)
    pic.Location<-Point(j*scale,i*scale)
    this.Controls.Add(pic)
    let arrS=System.IO.File.ReadAllLines(@"C:\Users\allen\Desktop\b.txt")
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

  override this.OnPaint e =
    
    let g = e.Graphics  
    g.Transform<-w2v
    
    let pt=Point(x,y)
    let l= transformP v2w  pt
    pic.Location<-Point(x,y)
    for a in 0..Height1-1 do (
        for b in 0..Width1-1 do(
          let pt= Point((b*scale),(a*scale))
          if m2DArray.[a,b]=1 then
              
              let l= transformP v2w pt
              g.FillRectangle(Brushes.SteelBlue,pt.X,pt.Y,scale-1,scale-1)
         //   else
               // g.FillRectangle(Brushes.DarkGray,pt.X,pt.Y,scale-1,scale-1)
             
                                      )    
                                 )
    base.OnPaint(e)



  override this.OnKeyDown e =
   currX<-x
   currY<-y
   match e.KeyCode with
    | Keys.W-> 
        pic.Image<-bmpBack
        if m2DArray.[i-1,j]=0 then 
                                    y<-y-gap;
                                    i<-i-1;
                                    translateW(0.f,single(scale))
                                   
    | Keys.S -> 
        pic.Image<-bmpFront
        if m2DArray.[i+1,j]=0 then
                                  i<-i+1
                                  y<-y+gap
                                  translateW(0.f,-single(scale))
    | Keys.A -> 
        pic.Image<-bmpLeft
        if m2DArray.[i,j-1]=0 then
                                     x<-x-gap 
                                     j<-j-1
                                     translateW(single(scale),0.f)
    | Keys.D ->
        pic.Image<-bmpRight
        if m2DArray.[i,j+1]=0 then
                                     x<-x+gap;
                                     j<-j+1 ;
                                     translateW(-single(scale),0.f)
    | _ ->()


   this.Invalidate()
   



let e= new Mappa(Dock=DockStyle.Fill)

f.Controls.Add(e)
e.Focus()