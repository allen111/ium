open System.Windows.Forms
open System.Drawing
let ni= new NotifyIcon()
//come far apparire una notifica
ni.BalloonTipText<-"ciao mondo"
ni.Visible<-true
ni.Icon<-(SystemIcons.Exclamation)
ni.ShowBalloonTip(20000)

