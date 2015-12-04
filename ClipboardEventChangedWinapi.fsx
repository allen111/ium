

open  System.Runtime.InteropServices;
open  System.Windows.Forms;
open  System.Drawing;



module climod =         
//  module private clim = 
    [<DllImport( "user32.dll")>]
    extern int SetClipboardViewer(int hWndNewViewer)

    [<DllImport("User32.dll", CharSet = CharSet.Auto)>]
    extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext)

    [<DllImport("user32.dll", CharSet = CharSet.Auto)>]
    extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam)

  
type ClipboardChangedEventArgs ()=
     let a=2
     //nel caso mettici qualcosa

type ClipboardAux()as this=
    inherit Form()
    let ClipboardChangedEvent=new Event<ClipboardChangedEventArgs>()
    let mutable nextClipboardViewer =IntPtr(1)
//    override this.OnLoad(e)=
    do 
        nextClipboardViewer<-IntPtr( climod.SetClipboardViewer(int this.Handle))
    
    [<System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")>]
    override this.WndProc (m : Message byref) =
        let WM_DRAWCLIPBOARD = 0x308;
        let WM_CHANGECBCHAIN = 0x030D;
        
        match m.Msg with
         |wm when wm=WM_DRAWCLIPBOARD->
            //fai quello che vuoi clipboard changed
            
            ClipboardChangedEvent.Trigger(new ClipboardChangedEventArgs())
            let tmo=climod.SendMessage(nextClipboardViewer,m.Msg,m.WParam,m.LParam)
            ()
         |wm when wm=WM_CHANGECBCHAIN->
            if m.WParam=nextClipboardViewer then
                nextClipboardViewer<-m.LParam
            else
                let tmp=climod.SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam)
                ()
         |_->base.WndProc(&m)


    override this.Dispose e=
        let t=climod.ChangeClipboardChain(this.Handle,nextClipboardViewer)
        ()

    member this.ClipboardChange=ClipboardChangedEvent.Publish        
        

// let nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle)


let aux=new ClipboardAux()

aux.ClipboardChange.Add(fun e->
   
//    printfn "%A" (Clipboard.GetText())
    if Clipboard.ContainsFileDropList() then
            let tmpFD=Clipboard.GetFileDropList()
            let a=tmpFD.GetEnumerator()
            while(a.MoveNext()) do
                let b=a.Current.Split('\\')
                printfn "%A" (b.[b.Length-1])

    )

