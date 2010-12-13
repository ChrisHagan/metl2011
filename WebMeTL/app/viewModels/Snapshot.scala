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
import java.text._
import java.awt.font._
import com.bric.geom._

object Snapshot{ 
    trait HistoricalItem { 
        val identity:String 
        def render(g:java.awt.Graphics2D)
        def getColor(color:String) = 
            if(color.startsWith("#"))
                Color.decode("0x"+color.drop(3).mkString)
            else
                color.split(" ").map(_.toInt) match{
                    case Array(red,green,blue,alpha) => new java.awt.Color(red,green,blue,alpha)
                }
    }
    case class HistoricalImage(identity:String,x:Int,y:Int,width:Int,height:Int,source:String)extends HistoricalItem {
        override def render(g:java.awt.Graphics2D) = {
            val img = resourceCache.getOrElse(source,ImageIO.read(WS.url(Application.server+source).authenticate(Application.username,Application.password).get.getStream)).asInstanceOf[java.awt.Image]
            resourceCache.put(source,img)
            g.drawImage(img,x,y,width,height,null)
        }
    }
    case class HistoricalInk(identity:String,color:String,thickness:Float,points:immutable.List[Array[Float]])extends HistoricalItem{
        override def render(g:java.awt.Graphics2D) ={
            g.setPaint(getColor(color))
            val pressure = 0.22
            g.setStroke(new java.awt.BasicStroke(thickness+pressure.toFloat))
            val vectorizer = new BasicVectorizer
            points.foreach(pts=>vectorizer.add(pts(0),pts(1),0))
            g.draw(vectorizer.getShape)
        }
    }

    case class HistoricalText(identity:String,width:Int,x:Float,y:Float,text:String,style:String,family:String,weight:String,size:Int,decoration:String,color:String) extends HistoricalItem{
        override def render(g:java.awt.Graphics2D) = {
            g.setPaint(getColor(color))
            val frc = g.getFontRenderContext()
            val font = new Font(family, weight match{
                case "Normal" => 
                    if(style.contains("Italic"))
                        Font.ITALIC
                    else
                        Font.PLAIN
                case "Bold" => 
                    if(style.contains("Italic"))
                        Font.BOLD + Font.ITALIC
                    else
                        Font.BOLD
            }, size)
            var _y = y
            text.split("\n").foreach(t=>{
                println("Rendering ",t)
                val styledText = new AttributedString(t)
                val stubLayout = new TextLayout(t,font,frc)
                println(stubLayout.getAscent,stubLayout.getDescent,stubLayout.getLeading)
                _y = _y + stubLayout.getAscent + stubLayout.getDescent
                t.length match{
                    case 0 => false
                    case _ => {
                        styledText.addAttribute(TextAttribute.FONT, font)
                        if(decoration.contains("Underline"))
                            styledText.addAttribute(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON, 0, t.length)
                        if(decoration.contains("Strikethrough"))
                            styledText.addAttribute(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON, 0, t.length)
                        width match{
                            case -1 => g.drawString(styledText.getIterator,x,_y)
                            case _ =>{
                                val styledTextIterator = styledText.getIterator()
                                val measurer = new LineBreakMeasurer(styledTextIterator, frc)
                                while (measurer.getPosition() < text.length()) {
                                    val textLayout = measurer.nextLayout(width)
                                    _y += textLayout.getAscent()
                                    textLayout.draw(g, x, _y)
                                    _y += textLayout.getDescent() + textLayout.getLeading()
                                }
                            }
                        }
                    }
                }
            })
        }
    }
    val resourceCache = collection.mutable.Map.empty[String,java.awt.Image]
    def png(width:Int,height:Int,messages:immutable.List[String]):BufferedImage = {
        println
        var preParser = mutable.ListBuffer.empty[HistoricalItem]
        val relevantNodes = Array("image", "ink", "textbox","dirtyInk", "dirtyImage","dirtyText")
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
            val farPoints = mutable.ListBuffer.empty[Array[Float]]
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
                    val points = pointText.split(" ").map(sPt=>sPt.toFloat).grouped(3).toList
                    points.foreach(p=> farPoints += p)
                    preParser += HistoricalInk(identity,color,thickness.toInt,points)
                }
                case "textbox"=>{
                  val width = (s \ "width").text match{
                      case "NaN"=> -1
                      case t => t.toInt
                  }
                  val height = (s \ "height").text match{
                      case "NaN"=> -1
                      case t => t.toInt
                  }
                  val text = (s \ "text").text
                  val x = (s \ "x").text.toFloat
                  val y = (s \ "y").text.toFloat
                  val style = (s \ "style").text
                  val family = (s \ "family").text
                  val weight = (s \ "weight").text
                  val size = (s \ "size").text.toInt
                  val decoration = (s \ "decoration").text
                  val color = (s \ "color").text
                  val identity = (s \ "identity").text
                  preParser.filter(_.identity == identity).foreach(identified=>preParser -= identified)
                  if(text.length > 0)
                      preParser += HistoricalText(identity,width,x,y,text,style,family,weight,size,decoration,color)
                }
                case "dirtyInk" | "dirtyImage" | "dirtyText" =>{
                    val identity = (s \ "identity").text
                    preParser.filter(_.identity == identity).foreach(identified=>preParser -= identified)
                }
            }
            farPoints
        })
        time("Parsed and measured")
        val flatmaxs = maxs.flatten
        val maxX = (flatmaxs.length match{
            case 0 => width
            case _ => flatmaxs.map(p=>p(0)).max
        }).toInt
        val maxY = (flatmaxs.length match{
            case 0 => height
            case _ => flatmaxs.map(p=>p(1)).max
        }).toInt
        time("Max dimensions calculated",maxX,maxY)
        val unscaledImage = new BufferedImage(maxX,maxY,BufferedImage.TYPE_3BYTE_BGR)
        val g = unscaledImage.createGraphics.asInstanceOf[Graphics2D]
        g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        g.setRenderingHint(RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY);
        g.setPaint(Color.white)
        g.fill(new Rectangle(0,0,maxX,maxY))
        preParser.filter(_.isInstanceOf[HistoricalImage]).foreach(_.render(g))
        preParser.filter(_.isInstanceOf[HistoricalText]).foreach(_.render(g))
        preParser.filter(_.isInstanceOf[HistoricalInk]).foreach(_.render(g))
        time("Finished painting unscaled") 
        val scaledImage = new BufferedImage(width,height,BufferedImage.TYPE_3BYTE_BGR)
        val scaledG = scaledImage.createGraphics.asInstanceOf[Graphics2D]
        scaledG.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        scaledG.setRenderingHint(RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_BICUBIC);
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
