open System.Windows.Forms
open System.Drawing


type ImageCombinator()as this=
    inherit UserControl()
    let images=new ResizeArray<Image>()
    let buttons=new ResizeArray<Region>()
    let mutable w=100
    let mutable h=100
    let mutable imgF=null
    let mutable prevImgs=new ResizeArray<Image>()
    


    let addPrev(is)=
        prevImgs<-is
        if prevImgs.Count=0 then
            printfn"mmm no imgs"
        else
            let mutable x=0
            for n in 0..prevImgs.Count-1 do
                let tmpReg=new Region(Rectangle(x,210,100,100))
                buttons.Add(tmpReg)
                x<-x+100


    let add(i:Image)=
        let tmp=images.Count
        match tmp with
            |0->
                images.Add(i)
                
                
            |1-> 
                images.Add(i)
                w<-w*2
            |2->
                images.Add(i)
                h<-h*2
            |3->
                images.Add(i)
            |_->failwith("troppe")
    
    let remove(i:Image)=
        let tmp=images.Count
        match tmp with
            |0->failwith("insieme vuoto")
            |1-> 
                let a=images.Remove(i)
                w<-100
                h<-100
            |2->
                let a=images.Remove(i)
                w<-w/2
            |3->
                let a=images.Remove(i)
                h<-h/2
            |4->
                let a=images.Remove(i)
                ()
            |_->failwith("troppe?")

    let paintImgs(g:Graphics,b:Bitmap)=
        g.Clear(Color.Black)
        let tmp=images.Count
        let w=100
        let h=100
        match tmp with
            |0->()
            |1-> 
                g.DrawImage(images.[0],0,0,w,h)
            |2->
                g.DrawImage(images.[0],0,0,w,h)
                g.DrawImage(images.[1],w,0,w,h)
            |3->
                g.DrawImage(images.[0],0,0,w,h)
                g.DrawImage(images.[1],w,0,w,h)
                g.DrawImage(images.[2],0,h,w,h)
            |4->
                g.DrawImage(images.[0],0,0,w,h)
                g.DrawImage(images.[1],w,0,w,h)
                g.DrawImage(images.[2],0,h,w,h)
                g.DrawImage(images.[3],w,h,w,h)
            |_->failwith("troppe?")
        imgF<-b

    let bt1=new Button(Dock=DockStyle.Bottom,Text="salva")
    do this.Controls.Add(bt1)
    do bt1.Click.Add(fun e->
        let fd=new SaveFileDialog()
        fd.DefaultExt<-"png"
        fd.Filter <- "PNG|*.png";
        fd.Title <- "Save an Image File";
        fd.FileName<-"Image.png"
        let x=fd.ShowDialog()
        if fd.FileName<>"" then
            let fs=fd.OpenFile()
            imgF.Save(fs,Imaging.ImageFormat.Png)
            fs.Close()
        )
    
    override this.OnPaint e=
        let bit=new Bitmap(w*2,h*2)
        let g=Graphics.FromImage(bit)
        paintImgs(g,bit)
        e.Graphics.DrawImage(bit,0,0)
        let mutable x=0
        prevImgs |>Seq.iter (fun b->
           if x<400 then
            e.Graphics.DrawImage(b,x,210,100,100)
            
            x<-x+100
            )
        printfn"%A" x
        e.Graphics.FillRectangle(Brushes.Aqua,x,210,50,100)



    member this.CombImage=add 
    member this.Remove=remove
    member this.AddImages=addPrev
