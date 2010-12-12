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
import scala.math._
import javax.imageio._

object Application extends Controller {
    val width = 200
    val height = 150
    val server = "https://deified.adm.monash.edu.au:1188"
    val structure = "https://deified.adm.monash.edu.au:1188/Structure"
    val history = "https://%s.adm.monash.edu.au:1749/%d/all.zip"
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
        val uri = history.format(server,jid)
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
    private def startStopwatch ={
        println
        stopWatch(new java.util.Date().getTime)_
    }
    private def stopWatch(start:Long)(args:Any*)=println(start,new java.util.Date().getTime - start,args.mkString(" "))
    trait HistoricalItem { 
        val identity:String 
        def render(g:java.awt.Graphics2D)
    }
    case class HistoricalImage(identity:String,x:Int,y:Int,width:Int,height:Int,source:String)extends HistoricalItem {
        override def render(g:java.awt.Graphics2D) = {
            val img = resourceCache.getOrElse(source,ImageIO.read(WS.url(server+source).authenticate(username,password).get.getStream)).asInstanceOf[java.awt.Image]
            resourceCache.put(source,img)
            g.drawImage(img,x,y,width,height,null)
        }
    }
    case class HistoricalInk(identity:String,color:String,thickness:Float,points:List[Array[Int]])extends HistoricalItem{
        override def render(g:java.awt.Graphics2D) ={
            color.split(" ").map(_.toInt) match{
                case Array(red,green,blue,alpha) => g.setPaint(new java.awt.Color(red,green,blue,alpha))
            }
            g.setStroke(new java.awt.BasicStroke(thickness+0.2f))
            points.sliding(2).foreach(pts=>
                pts.length match{
                    case 2 => g.draw(new java.awt.geom.Line2D.Double(pts(0)(0),pts(0)(1),pts(1)(0),pts(1)(1)))
                    case _ => false
                })
        }
    }
    val resourceCache = collection.mutable.Map.empty[String,java.awt.Image]
    def snapshot(width:Int=640, height:Int=320,server:String,slide:Int)={
        import java.awt._
        val preTime = startStopwatch
        var preParser = collection.mutable.ListBuffer.empty[HistoricalItem]
        val relevantNodes = Array("image", "ink", "dirtyInk", "dirtyImage")
        val messages = slideXmppMessages(server,slide).toList
        preTime("Got messages")
        val time = startStopwatch
        val nodes = 
            messages.map( message=>
                xml.XML.loadString(message)
                    .descendant
                    .filter((node:xml.Node)=> 
                        relevantNodes.contains(node.label)))
            .toList.flatten
        time("Filtered nodes")
        val maxs = nodes.map(s=>{
            val farPoints = collection.mutable.ListBuffer.empty[Array[Int]]
            s.label match{
                case "image"=> {
                    val source = (s \ "source").text
                    val width = (s \ "width").text.toDouble.toInt
                    val height = (s \ "height").text.toDouble.toInt
                    val x = (s \ "x").text.toDouble.toInt
                    val y = (s \ "y").text.toDouble.toInt
                    val identity = (s \ "identity").text
                    farPoints += Array(x+width,y+height)
                    preParser += HistoricalImage(identity,x,y,width,height,source)
                }
                case "ink"=>{
                    val identity = (s \ "checksum").text
                    val color = (s \ "color").text                
                    val thickness = (s \ "thickness").text.toFloat
                    val pointText = (s \ "points").text
                    val points = pointText.split(" ").map(sPt=>sPt.toDouble.toInt).grouped(3).map(_.take(2)).toList
                    points.foreach(p=> farPoints += p)
                    preParser += HistoricalInk(identity,color,thickness.toInt,points)
                }
                case "dirtyInk" | "dirtyImage" =>{
                    val identity = (s \ "identity").text
                    preParser.filter(_.identity == identity).foreach(preParser.remove(_))
                }
            }
            farPoints
        })
        time("Parsed and measured")
        val flatmaxs = maxs.flatten
        val maxX = flatmaxs.length match{
            case 0 => width
            case _ => flatmaxs.map(p=>p(0)).max
        }
        val maxY = flatmaxs.length match{
            case 0 => height
            case _ => flatmaxs.map(p=>p(1)).max
        }
        time("Max dimensions calculated",maxX,maxY)
        val unscaledImage = new BufferedImage(maxX,maxY,BufferedImage.TYPE_INT_RGB)
        val g = unscaledImage.createGraphics.asInstanceOf[Graphics2D]
        g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        g.setPaint(Color.white)
        g.fill(new Rectangle(0,0,maxX,maxY))
        preParser.filter(_.isInstanceOf[HistoricalImage]).foreach(_.render(g))
        preParser.filter(_.isInstanceOf[HistoricalInk]).foreach(_.render(g))
        val scaledImage = new BufferedImage(width,height,BufferedImage.TYPE_INT_RGB)
        val scaledG = scaledImage.createGraphics.asInstanceOf[Graphics2D]
        scaledG.setPaint(Color.white)
        scaledG.fill(new Rectangle(0,0,width,height))
        val xScale = width.toDouble / maxX.toDouble
        val yScale = height.toDouble / maxY.toDouble
        val aspectScale = Math.min(xScale, yScale);
        val xOffset = ((width - maxX * aspectScale) / 2).toInt
        scaledG.drawImage(unscaledImage,xOffset,0,xOffset+(maxX * aspectScale).toInt,(maxY * aspectScale).toInt,0,0,maxX,maxY,null)
        time("Finished painting scaled") 
        javax.imageio.ImageIO.write(scaledImage, "png", response.out)
        request.contentType = "image/png"
        time("Response written") 
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
