package MeTL
import net.liftweb.json._
import net.liftweb.json.JsonDSL._
import net.liftweb.json.JsonAST._
import scala.math._
import scala.collection._

abstract class Clump(){
    def duration:JValue
    def toJson:JValue
    def toJsonSummary:JValue
    def toJsonFull:JValue=
        JObject(
            List(
                JField("duration", duration),
                JField("summary", toJsonSummary),
                JField("details", toJson)))
}
case class SimpleClump(representativeSample:Seq[Message]) extends Clump{
    val sortedSample = representativeSample.toList.sort((a,b) => a.timestamp < b.timestamp)
    val timestamps = sortedSample.map(_.timestamp)
    val _duration = representativeSample.length match{
        case 0 | 1 => 0
        case _ => timestamps.last - timestamps.first
    }
    override def duration:JValue = JInt(_duration)
    override def toJson:JValue = representativeSample.map(_.toJson)
    def toJsonSummary:JValue={
        (sortedSample.foldLeft(
            ClumpSummary(-1,_duration,SortedSet.empty[String],0,0,0,SortedSet.empty[Int]))
            ((acc,message)=> acc.merge(message))).toJson
    }
}
case class DetailedClump(clumpedDetail:Seq[Seq[Message]]) extends Clump{
    override def duration:JValue ={
        val times = clumpedDetail.flatMap(c=>c.map(c1=>c1.timestamp))
        JInt(times.length match{
            case 0 | 1 => 0
            case _ =>times.max - times.min
        })
    }
    override def toJson:JValue = clumpedDetail.map(clump=>SimpleClump(clump).toJson)
    def toJsonSummary:JValue = clumpedDetail.map(clump=>SimpleClump(clump).toJsonSummary)
}
case class ClumpSummary(timestamp:Long,duration:Long,authors:SortedSet[String],ink:Int,image:Int,text:Int,slides:SortedSet[Int]){
    def merge(other:Message)=ClumpSummary(
        if(timestamp < 0) other.timestamp else timestamp,
        duration,
        authors + other.author,
        ink + (if(other.name == "ink") 1 else 0),
        image + (if(other.name == "image") 1 else 0),
        text + (if(other.name == "text") 1 else 0),
        slides + other.slide
    )
    def toJson={
        JObject(
            List(
                JField("timestamp",JInt(timestamp)),
                JField("duration",JInt(duration)),
                JField("authors",JArray(authors.toList.map(a=>JString(a)).toList)),
                JField("ink",JInt(ink)),
                JField("image",JInt(image)),
                JField("text",JInt(text)),
                JField("slides",JArray(slides.toList.map(s=>JInt(s))))
            ))
    }
}
object MessageReductor{
    def clumpSlides(data:Seq[Message]):Clump ={ 
        DetailedClump(data.map(_.slide).distinct.map(slide=>data.filter(d=>d.slide == slide)))
    }
    def clump(data:Seq[Message]):Clump = clump(data,false)
    def clump(data:Seq[Message], retainDetail:Boolean):Clump ={
    val sortedData = data.toList.sort((a,b) => a.timestamp < b.timestamp)
        if(sortedData.length <= 2)
            if(retainDetail)
                DetailedClump(List(sortedData))
            else
                SimpleClump(sortedData)
        else
            clumpStructure(sortedData, if(retainDetail) doDetailedClump else doClump)
    }
    private def clumpStructure(data:Seq[Message], clumper:(Seq[Message],Int)=>Clump):Clump={
        val width = 1024
        val dotRadius = 25
        val dotsAcrossScreen = width / dotRadius
        val times = data.map(_.timestamp)
        val visualThreshold = ((times.max - times.min) / dotsAcrossScreen).toInt
        clumper(data, visualThreshold)
    }
    private def doClump(all:Seq[Message], threshold:Int):Clump={
        val sortedMessages = all.toList.sort((a,b)=>a.timestamp < b.timestamp)
        SimpleClump(doClump(sortedMessages,List.empty[Message], threshold).reverse)
    }
    private def doClump(rest:Seq[Message],acc:List[Message], threshold:Int):List[Message]={
        rest match{
            case List() => acc
            case head::tail=>{
                val mark = head.timestamp 
                val marked = rest.takeWhile(m=>abs(m.timestamp - mark) <= threshold)
                doClump(rest.slice(marked.size, rest.length), marked.last :: acc, threshold)
            }
        }  
    }
    private def doDetailedClump(all:Seq[Message], threshold:Int):DetailedClump={
        val sortedMessages = all.toList.sort((a,b)=>a.timestamp < b.timestamp)
        DetailedClump(doDetailedClump(sortedMessages,List.empty[Seq[Message]], threshold).reverse)
    }
    private def doDetailedClump(rest:Seq[Message],acc:List[Seq[Message]], threshold:Int):List[Seq[Message]]={
        rest match{
            case List() => acc
            case head::tail=>{
                val mark = head.timestamp 
                val marked = rest.takeWhile(m=>abs(m.timestamp - mark) <= threshold)
                doDetailedClump(rest.slice(marked.size, rest.length), marked :: acc, threshold)
            }
        }
    }
}
case class Message(name:String, timestamp:Long,slide:Int,author:String,standing:Int){
    def toJson = JObject(List(
        JField("contentType", JString(name)),
        JField("timestamp", JInt(timestamp)),
        JField("slide", JInt(slide)),
        JField("author", JString(author)),
        JField("standing", JInt(standing))
    ))
    override def toString = author+"@"+timestamp.toString
}
case class Quiz(title:String, id:Long,timestamp:Long, conversation:Int, author:String, options:Seq[QuizOption]){
    def toJson = JObject(List(
        JField("title",JString(title)),
        JField("timestamp",JInt(timestamp)),
        JField("id",JInt(id)),
        JField("conversation",JInt(conversation)),
        JField("author",JString(author)),
        JField("options",JArray(options.map(_.toJson).toList))
    ))
}
object QuizOption{
    def parse(message:xml.Node)={
        val name = (message \ "name").text
        val text = (message \ "text").text
        val correct = (message \ "correct").text.toBoolean
        val color = (message \ "color").text
        QuizOption(name,text,correct,color)
    }
}
case class QuizOption(name:String,text:String,correct:Boolean,color:String){
    def toJson = JObject(List(
        JField("name",JString(name)),
        JField("text",JString(text)),
        JField("correct",JBool(correct)),
        JField("color",JString("rgb(%s)".format(color.split(" ").take(3).mkString(","))))))
}
