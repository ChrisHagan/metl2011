package utils
object Stopwatch{
    def start = stopWatch(new java.util.Date().getTime)_
    private def stopWatch(start:Long)(args:Any*)=println(start,new java.util.Date().getTime - start,args.mkString(" "))
}
