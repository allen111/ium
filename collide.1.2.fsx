open System.Windows.Forms
open System.Drawing
let f = new Form(Text="hello", TopMost=true,Height=720,Width=1280)
f.Show()
//questo va e l ho reimplementato usando oop
//adesso devi implementare la traslazione per spostare la mappa 

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

  let translateW (tx, ty) =
    w2v.Translate(tx, ty)
    v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)
  
  let transformP (m:Drawing2D.Matrix) (p:Point) =
    let a = [| Point( p.X,  p.Y) |]
    m.TransformPoints(a)
    a.[0]
 
  override this.OnLoad e =
   
    pic.ImageLocation<-"C:/Users/allen/Desktop/LinkFront.png"
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

  override this.OnPaint e =
    let g = e.Graphics  
    g.Transform<-w2v
    //g.FillRectangle(bru,x,y,20,20)
    let pt=Point(x,y)
    let l= transformP v2w  pt
    pic.Location<-Point(x,y)
    for a in 0..Height1-1 do (
        for b in 0..Width1-1 do(
          let pt= Point((b*scale),(a*scale))
          if m2DArray.[a,b]=1 then
              
              let l= transformP v2w pt
              g.FillRectangle(Brushes.SteelBlue,pt.X,pt.Y,scale-1,scale-1)
            else
                g.FillRectangle(Brushes.DarkGray,pt.X,pt.Y,scale-1,scale-1)
             
                                      )    
                                 )

  override this.OnKeyDown e =
   currX<-x
   currY<-y
   match e.KeyCode with
    | Keys.W when m2DArray.[i-1,j]=0-> 
        y<-y-gap;
        i<-i-1;
        pic.ImageLocation<-"C:/Users/allen/Desktop/LinkBack.png"
        translateW(0.f,single(scale))
    | Keys.S when m2DArray.[i+1,j]=0->
         y<-y+gap;
         i<-i+1;
         pic.ImageLocation<-"C:/Users/allen/Desktop/LinkFront.png"   
         translateW(0.f,-single(scale))
    | Keys.A when m2DArray.[i,j-1]=0->
         x<-x-gap ;
         j<-j-1;
         pic.ImageLocation<-"C:/Users/allen/Desktop/LinkLeft.png"   
         translateW(single(scale),0.f)
    | Keys.D when m2DArray.[i,j+1]=0->
         x<-x+gap;
         j<-j+1 ;
         pic.ImageLocation<-"C:/Users/allen/Desktop/LinkRight.png"
         translateW(-single(scale),0.f)
    | _ ->()

   this.Invalidate(Rectangle(currX,currY,gap,gap))
   this.Invalidate(Rectangle(x,y,gap,gap))
   this.Invalidate()
//   printf "%d  ," j
//   printfn "%d" i
//   printf "%d  ," x
//   printfn "%d" y
//  override this.OnMouseDown e=
//    let l= transformP v2w e.Location
//    printf "%d  ," l.X
//    printfn "%d" l.Y
    



let e= new Mappa(Dock=DockStyle.Fill)
f.Controls.Add(e)
e.Focus()
  