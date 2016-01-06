open System.Windows.Forms
open System.Drawing


type ImageCombinator()as this=
    inherit UserControl()
    do this.SetStyle(ControlStyles.AllPaintingInWmPaint 
                     ||| ControlStyles.OptimizedDoubleBuffer, true)
    let images=new ResizeArray<Image>()
    let buttons=new ResizeArray<Region>()
    let mutable w=100
    let mutable h=100
    let mutable imgF=null
    let mutable prevImgs=new ResizeArray<Image>()
    let mutable selected= Array.zeroCreate 0
    let mutable page =1
    let mutable idx=0
    let mutable nextReg=new Region(Rectangle(404,210,50,100))
    let startPrev=50
    let mutable prevReg=new Region(Rectangle(0,210,50,100))

    let addPrev(is)=
        prevImgs<-is
        if prevImgs.Count=0 then
            printfn"mmm no imgs"
        else
            selected<-Array.zeroCreate prevImgs.Count
            let mutable x=startPrev
            for n in 0..3 do
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
            |_->printfn"sono gia 4 " 
    
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
        if images.Count<>0 then
            imgF<-new Bitmap(images.[0].Width*2,images.[0].Height*2)
        else
            imgF<-new Bitmap(200,200)
        let gf=Graphics.FromImage(imgF)
        g.Clear(Color.Black)
        gf.Clear(Color.Black)
        let tmp=images.Count
        let w=100
        let h=100
        let mutable hf=100
        let mutable wf=100
        match tmp with
            |0->()
            |1-> 
                g.DrawImage(images.[0],0,0,w,h)
                hf<-images.[0].Height
                wf<-images.[0].Width
                gf.DrawImage(images.[0],0,0,wf,hf)

            |2->
                g.DrawImage(images.[0],0,0,w,h)
                g.DrawImage(images.[1],w,0,w,h)
                hf<-images.[0].Height
                wf<-images.[0].Width
                gf.DrawImage(images.[0],0,0,wf,hf)
                gf.DrawImage(images.[0],wf,0,wf,hf)


            |3->
                g.DrawImage(images.[0],0,0,w,h)
                g.DrawImage(images.[1],w,0,w,h)
                g.DrawImage(images.[2],0,h,w,h)
                hf<-images.[0].Height
                wf<-images.[0].Width
                gf.DrawImage(images.[0],0,0,wf,hf)
                gf.DrawImage(images.[0],wf,0,wf,hf)
                gf.DrawImage(images.[0],0,hf,wf,hf)
            |4->
                g.DrawImage(images.[0],0,0,w,h)
                g.DrawImage(images.[1],w,0,w,h)
                g.DrawImage(images.[2],0,h,w,h)
                g.DrawImage(images.[3],w,h,w,h)
                hf<-images.[0].Height
                wf<-images.[0].Width
                gf.DrawImage(images.[0],0,0,wf,hf)
                gf.DrawImage(images.[0],wf,0,wf,hf)
                gf.DrawImage(images.[0],0,hf,wf,hf)
                gf.DrawImage(images.[0],wf,hf,wf,hf)
            |_->failwith("troppe?")
        //imgF<-b

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
    override this.OnMouseUp e=
        buttons|>Seq.iteri (fun i b->
            if b.IsVisible e.Location then
                
                if selected.[i+idx]=0 then
                    add(prevImgs.[i+idx])
                    selected.[i+idx]<-1
                else
                    remove prevImgs.[i+idx]
                    selected.[i+idx]<-0
            
            )
        if nextReg.IsVisible(e.Location) && page<=prevImgs.Count/4 then
            
            page<-page+1
            idx<-idx+4
            
            let rem=idx-(prevImgs.Count)%4
            buttons.Clear()
            
            match rem with
            |1->buttons.Add(new Region(Rectangle(startPrev+0,210,100,100)));buttons.Add(new Region(Rectangle(startPrev+100,210,100,100)));buttons.Add(new Region(Rectangle(startPrev+200,210,100,100)))
            |2->buttons.Add(new Region(Rectangle(startPrev+0,210,100,100)));buttons.Add(new Region(Rectangle(startPrev+100,210,100,100)))
            |3->buttons.Add(new Region(Rectangle(startPrev+0,210,100,100)))
            |4->buttons.Clear()
            |_->()


        if prevReg.IsVisible(e.Location) && page>1 then
            page<-page-1
            idx<-idx-4
            
            
            buttons.Clear()
            let mutable x=startPrev
            for n in 0..3 do
                let tmpReg=new Region(Rectangle(x,210,100,100))
                buttons.Add(tmpReg)
                x<-x+100
            

        this.Invalidate()

    override this.OnPaint e=
        
        let bit=new Bitmap(200,200)
        let g=Graphics.FromImage(bit)
        
        paintImgs(g,bit)
        e.Graphics.DrawImage(bit,0,0,200,200)
        let mutable x=startPrev
        
        prevImgs |>Seq.iteri (fun i b->
           if i<page*4 && i>=(page-1)*4 then
                if selected.[i]=1 then
                    e.Graphics.DrawRectangle(Pens.Red,x-1,209,101,101)
                e.Graphics.DrawImage(b,x,210,100,100)
                
            
                x<-x+101
            )
        e.Graphics.FillRegion(Brushes.Aqua,prevReg)
        nextReg<-new Region(Rectangle(x,210,50,100))
        e.Graphics.FillRegion(Brushes.Aqua,nextReg)
        


    member this.CombImage=add 
    member this.Remove=remove
    member this.AddImages=addPrev
