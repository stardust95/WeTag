//--------------------------------------
//               PowerUI
//
//        For documentation or 
//    if you have any issues, visit
//        powerUI.kulestar.com
//
//    Copyright � 2013 Kulestar Ltd
//          www.kulestar.com
//--------------------------------------

namespace PowerUI{

	/// <summary>
	/// Handles the emphasis tag.
	/// </summary>

	public class EmTag:HtmlTagHandler{
		
		public override string[] GetTags(){
			return new string[]{"em"};
		}
		
		public override Wrench.TagHandler GetInstance(){
			return new EmTag();
		}
		
	}
	
}