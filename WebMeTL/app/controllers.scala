package controllers

import utils.Stemmer._
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
import MeTL._
import scala.math._

object Application extends Controller {
    val width = 200
    val height = 150
    val server = "https://reifier.adm.monash.edu.au:1188"
    val structure = "https://reifier.adm.monash.edu.au:1188/Structure"
    val history = "https://%s.adm.monash.edu.au:1749/%s/%d/all.zip"
    val username = "exampleUsername"
    val password = "examplePassword"
    val TEMP_FILE = "all.zip"
    val cachedConversations = loadConversations
    val contentWeight = mapRandom(cachedConversations, 1000)
    val authorsPerConversation = mapRandom(cachedConversations, 20)
    private def loadConversations = {
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
    def conversation(server:String,jid:Int)={
        val slides = cachedConversations(jid) \\ "slide"
        val messages = slides
            .filter(s=> (s \\ "type").text == "SLIDE")
            .map(s=>slide(server,(s \\ "id").text.toInt))
            .flatten
        val slideOrders = Map(slides.map(node=>((node \\ "id").text.toInt -> (node \\ "index").text.toInt)).toList:_*)
        val authors = Map(messages.map(m=>m.author).distinct.map(a=>(a->(Math.random*5).toInt)):_*)
        val relativizedMessages = messages.map(m=>Message(m.name,m.timestamp,slideOrders(m.slide), m.author, authors(m.author)))
        //val clump = SimpleClump(messages)
        //val clump = MessageReductor.clumpSlides(messages)
        val clump = MessageReductor.clump(relativizedMessages, true)
        pretty(render(clump.toJsonFull))
    }
    private def slideXmppMessages(server:String,jid:Int)={
        val uri = history.format(server,stem(jid),jid)
        println("Retrieving "+uri)
        val zipFuture = WS.url(uri).authenticate(username,password).get
        FileUtils.writeByteArrayToFile(new File(TEMP_FILE),IOUtils.toByteArray(zipFuture.getStream))
        val zipFile = new ZipFile(new File(TEMP_FILE))
        zipFile.getEntries
            .map(any => any.asInstanceOf[ZipArchiveEntry])
            .filter(zae=>zae.getName.endsWith(".xml"))
            .map(zae => IOUtils.toString(zipFile.getInputStream(zae))+"</logCollection>")
    }
    private def slide(server:String,jid:Int)={
        slideXmppMessages(server,jid)
            .map(detail => xml.XML.loadString(detail))
            .foldLeft(List.empty[Seq[Message]])((acc,item)=>{
                (item \\ "message").flatMap(message=>{
                    val timestamp = (message \ "@time").text.toLong
                    List("image","ink","text")
                        .flatMap(nodeName=>{
                            (message \\ nodeName).map(node=>{
                                val author = (node \ "author").text
                                Message(nodeName,timestamp,jid,author,0)
                           })
                        })
                }) :: acc
        }).flatten
    }
    def quizzes(server:String,jid:Int)={
        val qs = slideXmppMessages(server,jid)
            .map(d=>xml.XML.loadString(d))
            .foldLeft(List.empty[Quiz])((acc,item)=>{
                (item \\ "message").flatMap(message=>{
                    val timestamp = (message \ "@time").text.toLong
                    (message \ "quiz").map(quiz=>{
                        println(quiz)
                        val title = (quiz \ "title").text
                        val conversation = jid
                        val author = (quiz \ "author").text
                        val options = (quiz \ "quizOption").map(QuizOption.parse(_))
                        Quiz(title,timestamp,conversation,author,options)
                    })
                }).toList ::: acc
            })        
        pretty(render(JArray(qs.map(_.toJson))))
    }
    def snapshot(width:Int=640, height:Int=320,server:String,slide:Int) ={
        println("Snapshotting "+slide)
        val image = viewModels.Snapshot.png(width,height,slideXmppMessages(server,slide).toList)
        javax.imageio.ImageIO.write(image, "png", response.out)
        request.contentType = "image/png"
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
