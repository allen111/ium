open System.Windows.Forms
open System.Drawing
let f = new Form(Text="hello", TopMost=true)
f.Show()
//questo va e l ho reimplementato usando oop
//adesso devi implementare la traslazione per spostare la mappa e lo zoom ma mnee

type Mappa() =
  inherit UserControl()
   
  let mutable Height1=1
  let mutable Width1=1
  let gap=20
  let mutable i,j=1,1
  let mutable x=j*20
  let mutable y=i*20
  let mutable currX,currY=x,y
  let mutable stato=0
  let mutable bru= Brushes.Red
  let  mutable loaded = false
  let mutable pic= new PictureBox()
  let mutable m2DArray= Array2D.zeroCreate<int> 5 5
  
 
  override this.OnLoad e =
    pic.ImageLocation<-"C:/Users/allen/Desktop/8.png"
    pic.Size<-Size(20,20)
    pic.Location<-Point(20,20)
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
    //g.FillRectangle(bru,x,y,20,20)
    pic.Location<-Point(x,y)
    for a in 0..Height1-1 do (
        for b in 0..Width1-1 do(
          if m2DArray.[a,b]=1 then
              g.FillRectangle(Brushes.SteelBlue,b*20,a*20,19,19)
             
                                      )    
                                 )

  override this.OnKeyDown e =
   currX<-x
   currY<-y
   match e.KeyCode with
    | Keys.W when m2DArray.[i-1,j]=0-> y<-y-gap; i<-i-1
    | Keys.S when m2DArray.[i+1,j]=0-> y<-y+gap; i<-i+1
    | Keys.A when m2DArray.[i,j-1]=0-> x<-x-gap ; j<-j-1   
    | Keys.D when m2DArray.[i,j+1]=0-> x<-x+gap; j<-j+1 
    | _ ->()

   this.Invalidate(Rectangle(currX,currY,gap,gap))
   this.Invalidate(Rectangle(x,y,gap,gap))
    






let e= new Mappa(Dock=DockStyle.Fill)
f.Controls.Add(e)
e.Focus()
  