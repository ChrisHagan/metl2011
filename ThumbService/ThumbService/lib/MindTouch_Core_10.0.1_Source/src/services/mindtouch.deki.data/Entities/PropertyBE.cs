/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */

using MindTouch.Dream;

namespace MindTouch.Deki.Data {
    public class PropertyBE : ResourceBE {

        //--- Constructors ---
        public PropertyBE()
            : base() {
            ResourceType = Type.PROPERTY;
        }

        public PropertyBE(ResourceBE res)
            : base(res) { }

        //--- Methods ---
        public XUri Uri(XUri parentUri) {
            return parentUri.At("properties", XUri.DoubleEncodeSegment(Name));
        }

        public XUri UriInfo(XUri parentUri) {
            return Uri(parentUri).At("info");
        }

        public XUri UriContent(XUri parentUri) {
            return Uri(parentUri);
        }

    }
}
