package utils
import collection._
object Stemmer{
    def stem(path:String):String = {
        val padded = ("0"*(5 - path.length)) + path
        padded.takeRight(5).take(2).mkString
    }
    def reabsolutizeUri(server:String,uri:String,prefix:String="Resource")={
        val path = new java.net.URI(uri).getPath
        val pathparts = path.split("/").filter(_ != "").toList 
        var stemmed = pathparts match{
            case List(prefix,stemmed,stemmable,_*) if (stem(stemmable) == stemmed) =>{
                (List(prefix,stemmed,stemmable) ::: pathparts.drop(3)).mkString("/")
            }
            case List(prefix,unstemmed,_*)=>{
                (List(prefix,stem(unstemmed),unstemmed) ::: pathparts.drop(2)).mkString("/")
            }
            case List(noPrefix)=>noPrefix
        }
        "https://%s.adm.monash.edu:1188/%s".format(server,stemmed)
    }
}
