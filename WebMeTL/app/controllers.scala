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
import MeTL._
import java.awt.image._

object Application extends Controller {
    val width = 200
    val height = 150
    val server = "https://deified.adm.monash.edu.au:1188"
    val structure = "https://deified.adm.monash.edu.au:1188/Structure"
    val history = "https://deified.adm.monash.edu.au:1749/"
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
    def conversation(jid:Int)={
        val slides = cachedConversations(jid) \\ "slide"
        val messages = slides
            .filter(s=> (s \\ "type").text == "SLIDE")
            .map(s=>slide((s \\ "id").text.toInt))
            .flatten
        val slideOrders = Map(slides.map(node=>((node \\ "id").text.toInt -> (node \\ "index").text.toInt)).toList:_*)
        val authors = Map(messages.map(m=>m.author).distinct.map(a=>(a->(Math.random*5).toInt)):_*)
        val relativizedMessages = messages.map(m=>Message(m.name,m.timestamp,slideOrders(m.slide), m.author, authors(m.author)))
        //val clump = SimpleClump(messages)
        //val clump = MessageReductor.clumpSlides(messages)
        val clump = MessageReductor.clump(relativizedMessages, true)
        pretty(render(clump.toJsonFull))
    }
    private def slideXmppMessages(jid:Int)={
        val zipFuture = WS.url("%s/%s/all.zip".format(history,jid)).authenticate(username,password).get
        FileUtils.writeByteArrayToFile(new File(TEMP_FILE),IOUtils.toByteArray(zipFuture.getStream))
        val zipFile = new ZipFile(new File(TEMP_FILE))
        zipFile.getEntries
            .map(any => any.asInstanceOf[ZipArchiveEntry])
            .filter(zae=>zae.getName.endsWith(".xml"))
            .map(zae => IOUtils.toString(zipFile.getInputStream(zae))+"</logCollection>")
            .map(detail => xml.XML.loadString(detail))
    }
    private def slide(jid:Int)={
        slideXmppMessages(jid)
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
    def snapshot(jid:Int, width:Int=640, height:Int=320)={
        import java.awt._
        request.contentType = "image/png"
        val image = new BufferedImage(width,height,BufferedImage.TYPE_INT_RGB)
        val g = image.createGraphics.asInstanceOf[java.awt.Graphics2D]
        g.setPaint(Color.white)
        g.fill(new Rectangle(0,0,width,height))
        for(m <- slideXmppMessages(jid)){
            for(s <- (m \\ "image")){
                val source = (s \ "source").text
                val width = (s \ "width").text.toFloat.toInt
                val height = (s \ "height").text.toFloat.toInt
                val x = (s \ "x").text.toFloat.toInt
                val y = (s \ "y").text.toFloat.toInt
                val img = javax.imageio.ImageIO.read(WS.url(server+source).authenticate(username,password).get.getStream)
                g.drawImage(img,x,y,null)
            }
            for(s <- (m \\ "ink")){
                (s \ "color").text.split(" ").map(_.toInt) match{
                    case Array(red,green,blue,alpha) => g.setPaint(new java.awt.Color(red,green,blue,alpha))
                }
                g.setStroke(new BasicStroke((s \ "thickness").text.toInt))
                for(ps <- (s \ "points").text.split(" ").map(_.toFloat.toInt).grouped(3).grouped(2)){
                    ps match{
                        case Seq(Array(x1,y1,_),Array(x2,y2,_))=>{
                            println(x1,y1,x2,y2)
                            g.drawLine(x1,y1,x2,y2)
                        }
                        case Seq(_)=>false//We lose the last point if odd 
                    }
                }
            }
        }
        val baos = new java.io.ByteArrayOutputStream()
        javax.imageio.ImageIO.write(image, "png", baos)
        new java.io.ByteArrayInputStream(baos.toByteArray())
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
