open System.Windows.Forms
open System.Drawing
let f = new Form(Text="hello", TopMost=true)
f.Show()
let mutable grilled=false
let mutable arr= Array2D.zeroCreate<int> 0 0
let pnl= new Panel(Left=100,Top=20,BackColor=System.Drawing.Color.Aqua)
f.Controls.Add(pnl)


let mutable aaa=1

let mutable g = Graphics.FromHwnd(pnl.Handle)

let pnlButton= new Panel(Dock=DockStyle.Left,Width=101,BackColor=System.Drawing.Color.Brown)
f.Controls.Add(pnlButton)
let TextHeight= new TextBox(Dock=DockStyle.Top)
let TextWidth= new TextBox(Dock=DockStyle.Top)
let btnSize= new Button(Text="size",Dock=DockStyle.Top)
let labDeb= new Label(Text="click it 2 time because i'm dumb",Dock=DockStyle.Top)
pnlButton.Controls.Add(labDeb)
pnlButton.Controls.Add(btnSize)
pnlButton.Controls.Add(TextWidth)
pnlButton.Controls.Add(TextHeight)

let asasHeigt = ref 0
let asasWidth = ref 0
let mutable Heigt=5
let mutable Width=5
let mutable dragging = false

btnSize.Click.Add(fun e->
    if(System.Int32.TryParse(TextHeight.Text, asasHeigt)) then
        Heigt<-asasHeigt.Value
    if(System.Int32.TryParse(TextWidth.Text, asasWidth)) then
        Width<-asasWidth.Value
    g <- Graphics.FromHwnd(pnl.Handle)
    arr<-Array2D.zeroCreate<int> Heigt Width
   
    pnl.Height<-Heigt*20
    pnl.Width<-Width*20

    grilled<-true

    pnl.Invalidate()

)

pnl.Paint.Add(fun e->
  if grilled then
    for a in 0..Heigt-1 do (
            for b in 0..Width-1 do(
                if arr.[a,b]=1 then
                    g.FillRectangle(Brushes.Black,b*20,a*20,19,19)
                else
                    g.FillRectangle(Brushes.SteelBlue,b*20,a*20,19,19)
                                          )    
                                     )


)

pnl.MouseDown.Add(fun e->
   if grilled then
        dragging<-true
)

pnl.MouseUp.Add(fun e->
     
   if grilled && dragging then
        let tempX=e.Location.X/20
        let tempY=e.Location.Y/20
        if tempY<Heigt && tempX<Width && tempY>=0 && tempX>=0 then
          if e.Button=MouseButtons.Left then
            arr.[tempY,tempX]<-1
          else
            arr.[tempY,tempX]<-0  
          pnl.Invalidate(Rectangle(tempX*20,tempY*20,19,19))

        dragging<-false
                                 
)

pnl.MouseMove.Add(fun e->
     
     if grilled  && dragging then
        let tempX=e.Location.X/20
        let tempY=e.Location.Y/20

        if tempY<Heigt && tempX<Width && tempY>=0 && tempX>=0 then
            if e.Button=MouseButtons.Left then
                 arr.[tempY,tempX]<-1
            else
                arr.[tempY,tempX]<-0
            pnl.Invalidate(Rectangle(tempX*20,tempY*20,19,19))
                                 
)


let bt1= new Button(Text="Save",Dock=DockStyle.Bottom)
let LoadBtn= new Button(Text="Load",Dock=DockStyle.Bottom)
let lb1= new Label(Text="",Dock=DockStyle.Bottom)
let label= new Label(Text="Click me two time\n and use me after size\n 'couse i'm stupid",Dock=DockStyle.Bottom,Height=labDeb.Height*3)
pnlButton.Controls.Add(label)
pnlButton.Controls.Add(LoadBtn)
pnlButton.Controls.Add(bt1)
pnlButton.Controls.Add(lb1)

bt1.Click.Add(fun e->
if grilled then
    let mutable s=Heigt.ToString()+"\n"+Width.ToString()+"\n"
    for a in 0..Heigt-1 do (
                for b in 0..Width-1 do(
                                s<-s+arr.[a,b].ToString()+"\n"
                                
                                          )
      
                                     )

    let x=System.IO.File.WriteAllText(@"C:\Users\allen\Desktop\b.txt",s)
    lb1.Text<-"Result: problably done"
    
    aaa<-2
else
    lb1.Text<-"Result: nope"
)




LoadBtn.Click.Add(fun e ->
let arrS=System.IO.File.ReadAllLines(@"C:\Users\allen\Desktop\b.txt")

Heigt<-System.Int32.Parse(arrS.[0])
Width<-System.Int32.Parse(arrS.[1])
g <- Graphics.FromHwnd(pnl.Handle)
arr<-Array2D.zeroCreate<int> Heigt Width

pnl.Height<-Heigt*20
pnl.Width<-Width*20
let mutable count=2

for a in 0..Heigt-1 do (
       for b in 0..Width-1 do(
                arr.[a,b]<-System.Int32.Parse(arrS.[count])
                count<-count+1
                                          )    
                                     )

pnl.Invalidate()
)

(*
    LOAD/SAVE   done anche se posso migliorare il load 

*)
