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
    val server = "https://deified.adm.monash.edu.au:1188/Structure"
    val username = "exampleUsername"
    val password = "examplePassword"
    val TEMP_FILE = "all.zip"
    val history = preloadHistory
    val contentWeight = mapRandom(history, 1000)
    val authorsPerConversation = mapRandom(history, 20)
    private def preloadHistory = {
        val zipFuture = WS.url(server+"/all.zip").authenticate(username,password).get
        FileUtils.writeByteArrayToFile(new File(TEMP_FILE),IOUtils.toByteArray(zipFuture.getStream))
        val zipFile = new ZipFile(new File(TEMP_FILE))
        zipFile.getEntries
            .map(any => any.asInstanceOf[ZipArchiveEntry])
            .filter(zae=>zae.getName.endsWith("details.xml"))
            .map(zae => IOUtils.toString(zipFile.getInputStream(zae)))
            .map(detail => xml.XML.loadString(detail))
            .toList
    }
    private def mapRandom(history:Seq[xml.NodeSeq], max:Int = 10) = Map(history.map(c => ((c \ "jid").text, (Math.random * max).intValue.toString)):_*)
    def index = conversations
    def conversations = {
        val authorJson = authorSummaries(history).toString()
        Template(authorJson)
    }
    private def authorFrequencies(xs:Seq[xml.NodeSeq]) = {
        val authors = xs.map(x => (x \ "author").text)
        authors.groupBy(identity).mapValues(_.length)
    }
    private def elem(name:String, children:xml.Node*) = xml.Elem(null, name ,xml.Null,xml.TopScope, children:_*); 
    private def node(name:String, parent:xml.NodeSeq) = elem(name, xml.Text((parent \ name).text))
    private def authorSummary(kv:Pair[String,Int], xs:Seq[xml.NodeSeq]) = {
        val author = kv._1
        val freq = kv._2
        elem("conversationCount", xml.Text(freq.toString)) :: xs.filter(x=>(x \ "author").text == author).map(
            node=>{
                val conversationContentWeight = elem("contentVolume", xml.Text(contentWeight((node \ "jid").text)))
                val authorCount = elem("contentVolume", xml.Text(authorsPerConversation((node \ "jid").text)))
                val slideCount = elem("slideCount", xml.Text((node \ "slide").length.toString))
                <conversations>
                    {List("title","jid").map(name=>this.node(name, node)) ::: List(conversationContentWeight, authorCount, slideCount)}
                </conversations>;
            }).toList;
    }
    private def authorSummaries(xs:Seq[xml.NodeSeq]) = {
        val authorXmlList = authorFrequencies(xs).map(kv=>elem(kv._1, authorSummary(kv, xs):_*))
        val authors = 
        <conversationSummaries>
            {authorXmlList}
        </conversationSummaries>;
        val authorsJson = json.Xml.toJson(authors).transform {
            case JField("conversations", x: JObject) => JField("conversations", JArray(x :: Nil))
            case other => other
        }
        pretty(JsonAST.render(authorsJson))
    }
    def conversation(jid:Int)={
        val maybeXml = retrieveDetails(jid).map(renderDetails(_))
        maybeXml match{
            case Some(foundXml)=>{
                val resultXml = foundXml
                Template(resultXml)
            }
            case None=> "Conversation could not be found"
        }
    }
    private def retrieveDetails(id:Int):Option[xml.Node]={
        val uri = "%s/%s/details.xml".format(server,id)
        try{
            println(uri)
            Some(xml.XML.loadString(WS.url(uri).authenticate(username,password).get.getString))
        }
        catch{
            case e: Exception => None
        }
    }
    private def renderDetails(detail:xml.Node):xml.Node={
        val slides = (detail \\ "slide").map(slide =>{
            val thumbUri = "http://spacecaps.adm.monash.edu.au:8080/thumb/%s?width=%d&height=%d".format((slide \\ "id").text, width, height)
            <img src={thumbUri}></img>;
        })
        <div>
            <div>{(detail \\ "title").text}</div>
            {slides}
        </div>;
    }
}
