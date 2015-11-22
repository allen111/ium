open System.Windows.Forms
open System.Collections
open System.Drawing

let f= new Form(TopMost=true)
f.Show()


type aaa()=
    inherit UserControl()
    
    
    
    override this.OnMouseMove e=
        this.Invalidate()


    override this.OnPaint e=
        let g=e.Graphics
        let mutable i=PointF(10.f,10.f)
        let b=Clipboard.GetFileDropList()
        let a=b.GetEnumerator()
        while (a.MoveNext()) do
            g.DrawString(a.Current,this.Parent.Font,Brushes.Black,i)
            printfn "%A" (a.Current)
            i<-PointF(10.f,i.Y+10.f)

        



let sd2=new aaa(Dock=DockStyle.Fill)
f.Controls.Add(sd2)
f.Invalidate()
sd2.Focus()









    
