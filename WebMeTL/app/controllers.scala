package controllers

import play._
import play.mvc._
import play.libs._
import org.apache.commons.io._
import org.apache.commons.compress.archivers._
import org.apache.commons.compress.archivers.zip._
import java.io.{File}
import scala.xml.parsing._
import scala.collection.JavaConversions._
import org.json.simple._
import net.liftweb._
import net.liftweb.json._
import net.liftweb.json.JsonDSL._
import net.liftweb.json.JsonAST._
import collection.breakOut;

object Application extends Controller {
    val width = 200
    val height = 150
    val structure = "https://deified.adm.monash.edu.au:1188/Structure"
    val history = "https://deified.adm.monash.edu.au:1749/"
    val username = "exampleUsername"
    val password = "examplePassword"
    val TEMP_FILE = "all.zip"
    val cachedConversations = preloadConversations
    val contentWeight = mapRandom(cachedConversations, 1000)
    val authorsPerConversation = mapRandom(cachedConversations, 20)
    private def preloadConversations = {
        val zipFuture = WS.url(structure+"/all.zip").authenticate(username,password).get
        FileUtils.writeByteArrayToFile(new File(TEMP_FILE),IOUtils.toByteArray(zipFuture.getStream))
        val zipFile = new ZipFile(new File(TEMP_FILE))
        Map(zipFile.getEntries
            .map(any => any.asInstanceOf[ZipArchiveEntry])
            .filter(zae=>zae.getName.endsWith("details.xml"))
            .map(zae => IOUtils.toString(zipFile.getInputStream(zae)))
            .map(xmlString => xml.XML.loadString(xmlString))
            .map(detail => ((detail \ "jid").text.toInt -> detail)).toList:_*)
    }
    private def mapRandom(history:Map[Int,xml.NodeSeq], max:Int = 10) = history.map(kv => (kv._1 -> (Math.random * max).intValue.toString))
    def index = conversations
    def conversations = {
        val authorJson = authorSummaries(cachedConversations).toString()
        Template(authorJson)
    }
    def conversation(jid:Int)={
        val messages = (cachedConversations(jid) \\ "slide")
            .filter(s=> (s \\ "type").text == "SLIDE")
            .map(s=>slide((s \\ "id").text.toInt))
            .flatten
        pretty(render(clump(messages).map(_.toJson)))
    }
    private def clump(data:Seq[Message])={
        data
    }
    private case class Message(name:String, timestamp:Long,slide:Int,author:String){
        def toJson = JObject(List(
            JField("contentType", JString(name)),
            JField("timestamp", JInt(timestamp)),
            JField("slide", JInt(slide)),
            JField("author", JString(author))))
    }
    private def slide(jid:Int)={
        val zipFuture = WS.url("%s/%s/all.zip".format(history,jid)).authenticate(username,password).get
        FileUtils.writeByteArrayToFile(new File(TEMP_FILE),IOUtils.toByteArray(zipFuture.getStream))
        val zipFile = new ZipFile(new File(TEMP_FILE))
        zipFile.getEntries
            .map(any => any.asInstanceOf[ZipArchiveEntry])
            .filter(zae=>zae.getName.endsWith(".xml"))
            .map(zae => IOUtils.toString(zipFile.getInputStream(zae))+"</logCollection>")
            .map(detail => xml.XML.loadString(detail))
            .foldLeft(List.empty[Seq[Message]])((acc,item)=>{
                (item \\ "message").flatMap(message=>{
                    val timestamp = (message \ "@time").text.toLong
                    List("image","ink","text")
                        .flatMap(nodeName=>{
                            (message \\ nodeName).map(node=>{
                                val author = (node \ "author").text
                                Message(nodeName,timestamp,jid,author)
                           })
                        })
                }) :: acc
            }).flatten
    }
    private def authorFrequencies(xs:Map[Int,xml.NodeSeq]) = {
        val authors = xs.map(kv => (kv._2 \ "author").text)
        authors.groupBy(identity).mapValues(_.size)
    }
    private def elem(name:String, children:xml.Node*) = xml.Elem(null, name ,xml.Null,xml.TopScope, children:_*); 
    private def node(name:String, parent:xml.NodeSeq) = elem(name, xml.Text((parent \ name).text))
    private def authorSummary(kv:Pair[String,Int], xs:Map[Int,xml.NodeSeq]) = {
        val author = kv._1
        val freq = kv._2
        val count = elem("conversationCount", xml.Text(freq.toString))
        val conversationsBody = xs.filter(kv=>(kv._2 \ "author").text == author).map(
            kv=>{
                val node = kv._2
                val conversationContentWeight = elem("contentVolume", xml.Text(contentWeight((node \ "jid").text.toInt)))
                val authorCount = elem("authorCount", xml.Text(authorsPerConversation((node \ "jid").text.toInt)))
                val slideCount = elem("slideCount", xml.Text((node \ "slide").length.toString))
                <listing>
                    {List("title","jid").map(name=>this.node(name, node)) ::: List(conversationContentWeight, authorCount, slideCount)}
                </listing>;
            }).toList
        val conversations = <conversations>{conversationsBody}</conversations>;
        elem(author, List(count, conversations):_*)
    }
    private def authorSummaries(xs:Map[Int,xml.NodeSeq]) = {
        val authorXmlList = authorFrequencies(xs).map(kv=>authorSummary(kv, xs))
        val authors = 
        <conversationSummaries>
            {authorXmlList}
        </conversationSummaries>;
        val authorsJson = json.Xml.toJson(authors).transform {
            case JField("listing", x: JObject) => JField("listing", JArray(x :: Nil))
            case other => other
        }
        pretty(render(authorsJson))
    }
}
