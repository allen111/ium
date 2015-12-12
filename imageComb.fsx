open System.Windows.Forms
open System.Drawing

let f=new Form(TopMost=true)
f.Show()



let img1=Image.FromFile(@"C:\Users\allen\Pictures\a.png")

let w=200
let h=300
let bt1=new Bitmap(w*2,h)

f.Paint.Add(fun e->
    let g=Graphics.FromImage(bt1)
    g.Clear(Color.Black)
    g.DrawImage(img1,0,0,w,h)
    g.DrawImage(img1,w,0,w,h)
    e.Graphics.DrawImage(bt1,0,0)




)
bt1.Save(@"C:\Users\allen\Pictures\a1.png",Imaging.ImageFormat.Png)