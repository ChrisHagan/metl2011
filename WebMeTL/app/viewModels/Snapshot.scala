package viewModels
import java.awt._
import utils._
import javax.imageio._
import play._
import play.mvc._
import play.libs._
import controllers._
import scala.collection._
import java.awt.image._
import com.bric.geom._

object Snapshot{ 
    trait HistoricalItem { 
        val identity:String 
        def render(g:java.awt.Graphics2D)
    }
    case class HistoricalImage(identity:String,x:Int,y:Int,width:Int,height:Int,source:String)extends HistoricalItem {
        override def render(g:java.awt.Graphics2D) = {
            val img = resourceCache.getOrElse(source,ImageIO.read(WS.url(Application.server+source).authenticate(Application.username,Application.password).get.getStream)).asInstanceOf[java.awt.Image]
            resourceCache.put(source,img)
            g.drawImage(img,x,y,width,height,null)
        }
    }
    case class HistoricalInk(identity:String,color:String,thickness:Float,points:immutable.List[Array[Int]])extends HistoricalItem{
        override def render(g:java.awt.Graphics2D) ={
            color.split(" ").map(_.toInt) match{
                case Array(red,green,blue,alpha) => g.setPaint(new java.awt.Color(red,green,blue,alpha))
            }
            points.sliding(2).foreach(pts=>
                pts.length match{
                    case 2 =>{
                        //val pressure = 0.75 - (((pts(0)(2)+pts(1)(2)) / 2.0) / 255.0)
                        val pressure = 0.1
                        g.setStroke(new java.awt.BasicStroke(thickness+pressure.toFloat))
                        g.draw(new java.awt.geom.Line2D.Double(pts(0)(0),pts(0)(1),pts(1)(0),pts(1)(1)))
                    }
                    case _ => false
                })
        }
    }
    val resourceCache = collection.mutable.Map.empty[String,java.awt.Image]
    def png(width:Int,height:Int,messages:immutable.List[String]):BufferedImage = {    
        val preTime = Stopwatch.start
        var preParser = mutable.ListBuffer.empty[HistoricalItem]
        val relevantNodes = Array("image", "ink", "dirtyInk", "dirtyImage")
        preTime("Got messages")
        val time = Stopwatch.start
        val nodes = 
            messages.map( message=>
                xml.XML.loadString(message)
                    .descendant
                    .filter((node:xml.Node)=> 
                        relevantNodes.contains(node.label)))
            .toList.flatten
        time("Filtered nodes")
        val maxs = nodes.map(s=>{
            val farPoints = mutable.ListBuffer.empty[Array[Int]]
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
                    val points = pointText.split(" ").map(sPt=>sPt.toDouble.toInt).grouped(3).toList
                    points.foreach(p=> farPoints += p)
                    preParser += HistoricalInk(identity,color,thickness.toInt,points)
                }
                case "dirtyInk" | "dirtyImage" =>{
                    val identity = (s \ "identity").text
                    preParser.filter(_.identity == identity).foreach(identified=>preParser -= identified)
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
        time("Finished painting unscaled") 
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
        scaledImage
    }
}
