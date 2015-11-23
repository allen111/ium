open System.Windows.Forms
open System.Collections.Generic
open System.Drawing
open System.Text.RegularExpressions
let f= new Form(TopMost=true)
f.Show()


type aaa()as this=
    inherit UserControl()
    do this.SetStyle(ControlStyles.AllPaintingInWmPaint 
                     ||| ControlStyles.OptimizedDoubleBuffer, true)



    
        
    let t= new Timer(Interval=500)
    do
        t.Tick.Add(fun e->
            this.Invalidate()
            )
        t.Start()
    
    let mutable strlist=new List<string>()
    override this.OnMouseMove e=
        ()


    override this.OnKeyDown e =
        match e.KeyCode with
            |Keys.C->this.Invalidate()
            |_->()



    override this.OnPaint e=
        let g=e.Graphics
        let mutable i=PointF(10.f,10.f)
        let b=Clipboard.GetFileDropList()
        if Clipboard.ContainsText() then
            let c=Clipboard.GetText()
            g.DrawString(c,this.Parent.Font,Brushes.Black,i)
            i<-PointF(10.f,i.Y+20.f)
//        strlist.Add(c)
//        strlist |> Seq.iter (fun e->
//            g.DrawString(e,this.Parent.Font,Brushes.Black,i)
//            i<-PointF(10.f,i.Y+20.f) 
//            )
        let a=b.GetEnumerator()
        while (a.MoveNext()) do
            g.DrawString(a.Current,this.Parent.Font,Brushes.Black,i)
            i<-PointF(10.f,i.Y+20.f)
            let pattern = @"\d*\.png"
            let regex= new Regex(pattern,RegexOptions.IgnoreCase)
            let pattern1 = @"\d*\.jpg"
            let regex1= new Regex(pattern1,RegexOptions.IgnoreCase)
            
            if regex.IsMatch(a.Current) || regex1.IsMatch(a.Current) then
                
                let imgT=Image.FromFile(a.Current)
                g.DrawImage(imgT,RectangleF(i.X,i.Y,100.f,100.f))       
                i<-PointF(10.f,i.Y+100.f)
        
                

        (*string pattern = "d*";
Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

if (regex.IsMatch(input))*)



let sd2=new aaa(Dock=DockStyle.Fill)
f.Controls.Add(sd2)
f.Invalidate()
sd2.Focus()









    
