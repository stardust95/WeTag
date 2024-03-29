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

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PowerUI{
	
	/// <summary>
	/// A world UI is one which can be placed in a particular spot in the game world and seen with the game camera.
	/// For example, an in-game computer screen/ billboard etc. It has a pixel height and width, a document 
	/// (which works just like UI.document does) and can have its resolution and origin changed. The resolution defines
	/// how many pixels make up one world unit and the origin defines where the gameobjects origin is relative to the UI.
	/// By default, it's in the middle.
	/// </summary>
	
	public class WorldUI{
	
		/// <summary>A lookup used to quickly find a WorldUI from its transform. 
		/// Used for physics based input resolving.</summary>
		public static Dictionary<Transform,WorldUI> PhysicsLookup;
		/// <summary>True if any of the live WorldUI's are 'updateable'. These are pixel perfect or ones which are facing a camera.</summary>
		public static bool LiveUpdatablesAvailable;
		
		/// <summary>Updates all worldUI's. Called internally to update pixel perfect WorldUI's.</summary>
		public static void UpdateAll(){
		
			if(UI.FirstWorldUI!=null){
				
				WorldUI current=UI.FirstWorldUI;
				while(current!=null){
					current.Update();
					current=current.UIAfter;
				}
				
			}
			
		}
		
		/// <summary>Finds a WorldUI by name. Note that this is a linear scan so it's very wise to keep hold of references to your WorldUI's where possible.
		/// If multiple WorldUI's have the same name, the latest one created is returned.</summary>
		/// <param name="name">The WorldUI's name.</param>
		/// <returns>The latest WorldUI with this name, or null if not found.</returns>
		public static WorldUI Find(string name){
			
			// This function seeks "backwards" to find the latest one.
			WorldUI current=UI.LastWorldUI;
			
			while(current!=null){
				
				if(current.Name==name){
					return current;
				}
				
				current=current.UIBefore;
			}
				
			return null;
		}
		
		/// <summary>True if this UI is rendering flat.</summary>
		public bool Flat;
		/// <summary>The width/height ratio.</summary>
		public float Ratio=1f;
		/// <summary>The width of the UI in pixels.</summary>
		public int pixelWidth;
		/// <summary>The height of the UI in pixels.</summary>
		public int pixelHeight;
		/// <summary>The HTML document for this UI. Use this to edit the content.</summary>
		public Document document;
		/// <summary>The world space transform of the UI. Use this to move it around.</summary>
		public Transform transform;
		/// <summary>The game object that this UI is parented to.</summary>
		public GameObject gameObject;
		
		
		/// <summary>The name of this WorldUI. Use WorldUI.Find to obtain a WorldUI by name.</summary>
		public string Name;
		/// <summary>Does this WorldUI expire?
		public bool Expires;
		/// <summary>How long until this worldUI expires.</summary>
		public float ExpiresIn;
		/// <summary>For internal use. All WorldUI's are stored as a linked list for updates.
		/// This is the next one in the list.</summary>
		public WorldUI UIAfter;
		/// <summary>For internal use. All WorldUI's are stored as a linked list for updates.
		/// This is the one before this in the list.</summary>
		public WorldUI UIBefore;
		/// <summary>The height of the UI in pixels as a float.</summary>
		public float PixelHeightF;
		/// <summary>The renderer that renders this UI.</summary>
		public Renderman Renderer;
		/// <summary>The camera to look at. Camera.main is used if this is null and AlwaysFaceCamera is true.</summary>
		public Camera CameraToFace;
		/// <summary>True if the resolution of this WorldUI is automatically updated such that the UI is always pixel perfect.</summary>
		private bool IsPixelPerfect;
		/// <sumamry>True if this UI should face the main camera.</summary>
		public bool AlwaysFaceCamera;
		/// <summary>An event called when this WorldUI expires. Returning false prevents the default destroy action.</summary>
		public WorldUIExpiryEvent OnExpire;
		/// <summary>Cached percentage of the screen that a pixel perfect WorldUI should take up.</summary>
		private float ScreenSpaceProportion;
		/// <summary>The location of the origin in pixels. Set internally; see <see cref="PowerUI.WorldUI.SetOrigin"/>.</summary>
		public Vector2 WorldScreenOrigin=Vector2.zero;
		/// <summary>The location of the gameobjects origin relatively. Set internally; see <see cref="PowerUI.WorldUI.SetOrigin"/>.</summary>
		public Vector2 OriginLocation=new Vector2(0.5f,0.5f);
		
		
		/// <summary>Creates a new World UI with 100x100 pixels of space and a name of "new World UI".
		/// The gameobjects origin sits at the middle of the UI by default. See <see cref="PowerUI.WorldUI.SetOrigin"/>. 
		/// By default, 100 pixels are 1 world unit. See <see cref="PowerUI.WorldUI.SetResolution"/>.</summary>
		public WorldUI():this("new World UI",100,100){}
		
		/// <summary>Creates a new World UI with 100x100 pixels of space and the given name.
		/// The gameobjects origin sits at the middle of the UI by default. See <see cref="PowerUI.WorldUI.SetOrigin"/>. 
		/// By default, 100 pixels are 1 world unit. See <see cref="PowerUI.WorldUI.SetResolution"/>.</summary>
		/// <param name="name">The name for the UI's gameobject.</param>
		public WorldUI(string name):this(name,100,100){}
		
		/// <summary>Creates a new World UI with the given pixels of space and a name of "new World UI".
		/// The gameobjects origin sits at the middle of the UI by default. See <see cref="PowerUI.WorldUI.SetOrigin"/>. 
		/// By default, 100 pixels are 1 world unit. See <see cref="PowerUI.WorldUI.SetResolution"/>.</summary>
		/// <param name="widthPX">The width in pixels of this UI.</param>
		/// <param name="heightPX">The height in pixels of this UI.</param>
		public WorldUI(int widthPX,int heightPX):this("new World UI",widthPX,heightPX){}
		
		/// <summary>Creates a new World UI with the given pixels of space and a given name.
		/// The gameobjects origin sits at the middle of the UI by default. See <see cref="PowerUI.WorldUI.SetOrigin"/>. 
		/// By default, 100 pixels are 1 world unit. See <see cref="PowerUI.WorldUI.SetResolution"/>.</summary>
		/// <param name="name">The name for the UI's gameobject.</param>
		/// <param name="widthPX">The width in pixels of this UI.</param>
		/// <param name="heightPX">The height in pixels of this UI.</param>
		public WorldUI(string name,int widthPX,int heightPX){
			// Start the UI:
			UI.Start();
			
			// Create the gameobject:
			gameObject=new GameObject();
			gameObject.name=name;
			
			// Grab the name:
			Name=name;
			
			transform=gameObject.transform;
			Renderer=new Renderman(this);
			SetDepthResolution(0.01f);
			
			// Apply the default scale:
			transform.localScale=new Vector3(1/100f,1/100f,1f);
			
			document=Renderer.RootDocument;
			
			// Add it to the UI update linked list:
			if(UI.FirstWorldUI==null){
				UI.FirstWorldUI=UI.LastWorldUI=this;
			}else{
				UIBefore=UI.LastWorldUI;
				UI.LastWorldUI=UI.LastWorldUI.UIAfter=this;
			}
			
			SetDimensions(widthPX,heightPX);
			
			SetInputMode(PowerUI.Input.WorldInputMode);
		}
		
		/// <summary>True if the resolution of this WorldUI is automatically updated such that the UI is always pixel perfect.</summary>
		public bool PixelPerfect{
			get{
				return IsPixelPerfect;
			}
			set{
				if(IsPixelPerfect==value){
					return;
				}
				
				IsPixelPerfect=value;
				
				if(IsPixelPerfect){
					
					if(!AlwaysFaceCamera){
						FaceCamera();
					}
					
					UI.document.OnResized+=MainScreenSizeChanged;
					
					// Make sure we're up to date:
					MainScreenSizeChanged();
					
				}else{
					UI.document.OnResized-=MainScreenSizeChanged;
				}
				
			}
		}
		
		/// <summary>Call this if you change the field of view of the camera looking at a pixel perfect WorldUI.</summary>
		public void CameraChanged(){
			CameraToFaceChanged();
		}
		
		/// <summary>Called when the game screen changes size. Used by pixel perfect WorldUI's.</summary>
		private void MainScreenSizeChanged(){
			
			Camera camera=CameraToFace;
				
			if(camera==null){
				camera=Input.CameraFor3DInput;
				
				if(camera==null){
					camera=Camera.main;
				}
				
			}
			
			// Update the screen space proportion.
			
			// First get the 'growth' rate of the screen height. 
			ScreenSpaceProportion=Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView/2f ) * 2f;
			
			// Divide it by the screen height in pixels for it in world units:
			ScreenSpaceProportion/=ScreenInfo.ScreenYFloat;
			
		}
		
		/// <summary>How this WorldUI renders images; either on an atlas or with them 'as is'.
		/// Default is Atlas.</summary>
		public RenderMode RenderMode{
			get{
				return Renderer.RenderMode;
			}
			set{
				Renderer.RenderMode=value;
			}
		}
		
		[Obsolete("Atlases are now always global. If you wish to define their size, see AtlasStacks instead.")]
		public int AtlasSize;
		
		/// <summary>Flushes resolution changes. Use this after a set resolution method if you want to see your
		/// changes on the next update. Note that this is not needed when you've just created your WorldUI or if you're
		/// also changing any innerHTML or styles at the same time. Internally this just generates a layout request.</summary>
		public void UpdateResolution(){
			Renderer.RequestLayout();
		}
		
		/// <summary>Called when the input mode changes. This makes sure all active 
		/// batches are set to the correct mode (i.e. physics or screen). Default is None.</summary>
		/// <param name="mode">The new mode to use.</param>
		public void SetInputMode(InputMode mode){
			if(gameObject==null){
				return;
			}
			
			Renderer.SetInputMode(mode);
			
			if(mode!=InputMode.None && transform!=null){
				// Add to the global lookup for faster resolving.
				if(PhysicsLookup==null){
					PhysicsLookup=new Dictionary<Transform,WorldUI>();
				}
				
				PhysicsLookup[transform]=this;
			}else{
				PhysicsLookup=null;
			}
			
			if(Renderer.PhysicsModeCollider!=null){
				
				// Scale the collider:
				Renderer.PhysicsModeCollider.localScale=new Vector3((float)pixelWidth,PixelHeightF,0.01f);
				
			}
			
		}
		
		/// <summary>The layer to put this worldUI in.</summary>
		public int Layer{
			set{
				RenderWithCamera(value);
			}
			get{
				return gameObject.layer;
			}
		}
		
		/// <summary>Puts all batches of this renderer into the given unity layer.</summary>
		/// <param name="id">The ID of the unity layer.</param>
		public virtual void RenderWithCamera(int id){
			Renderer.RenderWithCamera(id);
			gameObject.layer=id;
		}
		
		/// <summary>The collider's transform if WorldUI's are in Screen input mode. This will have a BoxCollider attached.</summary>
		public Transform PhysicsModeCollider{
			get{
				return Renderer.PhysicsModeCollider;
			}
		}
		
		/// <summary>Sets how many world units are used between elements at different depths. Default is 0.01.
		/// You'll generally want this to be as small as possible to make the UI appear flat.
		/// Too small though and you'll get z-fighting. You can also use large values if you want to achieve a unique effect.</summary>
		/// <param name="gaps">The distance between elements to use.</param>
		public void SetDepthResolution(float gaps){
			Renderer.DepthResolution=gaps;
		}
		
		/// <summary>Sets how many Pixels Per World unit this renderer uses. Maps directly to applying a scale.
		/// Default is 100. The actual world space size is dictated by this and <see cref="PowerUI.WorldUI.SetDimensions"/>.
		/// The amount of pixels and pixels per world unit (resolution).</summary>
		/// <param name="ppw">Pixels per world unit to use for both x and y.</param>
		public void SetResolution(int ppw){
			SetResolution(ppw,ppw);
		}
		
		/// <summary>Sets how many Pixels Per World unit this renderer uses. Maps directly to applying a scale.
		/// Default is 100. The actual world space size is dictated by this and <see cref="PowerUI.WorldUI.SetDimensions"/>.
		/// The amount of pixels and pixels per world unit (resolution).</summary>
		/// <param name="ppw">Pixels per world unit to use for both x and y.</param>
		public void SetResolution(float ppw){
			transform.localScale=new Vector3(1f/ppw,1f/ppw,1f);
		}
		
		/// <summary>Sets how many Pixels Per World unit this renderer uses, allowing for distortion. Maps directly to applying a scale.
		/// Default is 100 on each axis. The actual world space size is dictated by this
		/// and <see cref="PowerUI.WorldUI.SetDimensions"/>. The amount of pixels and pixels per world unit (resolution).</summary>
		/// <param name="ppwW">Pixels per world unit to use for the x axis.</param>
		/// <param name="ppwH">Pixels per world unit to use for the y axis.</param>
		public void SetResolution(int ppwW,int ppwH){
			transform.localScale=new Vector3(1f/ppwW,1f/ppwH,1f);
		}
		
		/// <summary>The amount of world units per pixel. This is just the transform scale.</summary>
		public Vector2 WorldPerPixel{
			get{
				if(transform==null){
					return new Vector2(1/100f,1/100f);
				}
				
				Vector3 scale=transform.localScale;
				return new Vector2(scale.x,scale.y);
			}
		}
		
		/// <summary>The size of the screen in world units.</summary>
		public Vector2 WorldScreenSize{
			get{
				if(transform==null){
					return new Vector2((float)pixelWidth/100f,(float)pixelHeight/100f);
				}
				
				Vector3 scale=transform.localScale;
				return new Vector2(scale.x*pixelWidth,scale.y*pixelHeight);
			}
		}
		
		/// <summary>Sets how many pixels of space this renderer has. The actual world space size is dictated by this and
		/// <see cref="PowerUI.WorldUI.SetResolution"/>. The amount of pixels and pixels per world unit (resolution).</summary>
		/// <param name="widthPX">The width in pixels.</param>
		/// <param name="heightPX">The height in pixels.</param>
		public virtual void SetDimensions(int widthPX,int heightPX){
			if(widthPX!=pixelWidth){
				pixelWidth=widthPX;
				document.html.style.width=widthPX+"px";
			}
			
			if(heightPX!=pixelHeight){
				pixelHeight=heightPX;
				PixelHeightF=(float)pixelHeight;
				document.html.style.height=heightPX+"px";
			}
			
			// Update ratio:
			Ratio=(float)pixelWidth / PixelHeightF;
			
			// Reset the origin position:
			SetOrigin(OriginLocation.x,OriginLocation.y);
		}
		
		/// <summary>Sets the text filter mode. Note: shared fonts will be affected in all other UI's.</summary>
		public FilterMode TextFilterMode{
			get{
				return Renderer.TextFilterMode;
			}
			set{
				Renderer.TextFilterMode=value;
			}
		}
		
		/// <summary>Sets the location of the gameobjects origin relatively.</summary>
		/// <param name="x">The x coordinate of the origin as a value from 0->1. Zero is the left edge. Default 0.5.</param>
		/// <param name="y">The y coordinate of the origin as a value from 0->1. Zero is the bottom edge. Default is 0.5.</param>
		public virtual void SetOrigin(float x,float y){
			OriginLocation.x=x;
			OriginLocation.y=y;
			
			WorldScreenOrigin=new Vector2(-((float)pixelWidth)*x,
										  -PixelHeightF*y
										 );
										 
			Renderer.RelocateCollider();
		}
		
		/// <summary>Called when CameraToFace changes by calling a FaceCamera overload.</summary>
		private void CameraToFaceChanged(){
			if(IsPixelPerfect){
				MainScreenSizeChanged();
			}
		}
		
		/// <summary>Makes this UI always face the given camera.</summary>
		/// <param name="cameraToFace">The camera to face.</param>
		public void FaceCamera(Camera cameraToFace){
			CameraToFace=cameraToFace;
			AlwaysFaceCamera=true;
			
			CameraToFaceChanged();
		}
		
		/// <summary>Makes this UI face the main camera until told to stop.
		/// Note that this only needs to be called once.</summary>
		public void FaceCamera(){
			AlwaysFaceCamera=true;
			CameraToFace=null;
			CameraToFaceChanged();
		}
		
		/// <summary>Stops making this UI face a camera.</summary>
		public void StopFacingCamera(){
			AlwaysFaceCamera=false;
			CameraToFace=null;
			CameraToFaceChanged();
		}
		
		/// <summary>Updates this UI. Called internally by UI.Update.</summary>
		public void Update(){
			if(gameObject==null){
				Destroy();
				return;
			}
			
			if(AlwaysFaceCamera){
				// Simply rotate such that it's rotation matches the cameras.
				
				Camera camera=CameraToFace;
				
				if(camera==null){
					camera=Input.CameraFor3DInput;
					
					if(camera==null){
						camera=Camera.main;
					}
					
				}
				
				transform.rotation=camera.transform.rotation;
				
			}
			
			if(IsPixelPerfect){
				
				Camera camera=CameraToFace;
				
				if(camera==null){
					camera=Input.CameraFor3DInput;
					
					if(camera==null){
						camera=Camera.main;
					}
					
				}
				
				
				
				// How far away is the camera?
				float depth=Vector3.Distance(camera.transform.position,transform.position);
				
				// The following is highly optimised - almost everything cancels out or is a constant.
				
				// The screen height and width at the given depth:
				// float screenHeight = depth * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView/2f ) * 2f
				// float aspect = (float)ScreenInfo.ScreenX / ScreenInfo.ScreenYFloat;
				// float screenWidth = screenHeight * aspect;
				
				// Next, a percentage of the screen size this world UI should take up:
				// Vector2 screenSpace=new Vector2(pixelWidth/(float)ScreenInfo.ScreenX,PixelHeightF/ScreenInfo.ScreenYFloat);
				
				// Therefore, the screen space being taken up in world units is..
				// float spaceTakenX=screenSpace.x * screenWidth;
				// float spaceTakenY=screenSpace.y * screenHeight;
				
				// But 1 pixel = 1 world unit at a scale of 1, so to apply the scale we divide by pixelWidth and height:
				// spaceTakenX=spaceTakenX/(float)pixelWidth;
				// spaceTakenY=spaceTakenY/PixelHeightF;
				
				// Thus making the scale new Vector3(spaceTakenX,spaceTakenY,1f).
				
				
				// Apply the proportions:
				transform.localScale=new Vector3(depth*ScreenSpaceProportion,depth*ScreenSpaceProportion,1f);
				
			}
			
		}
		
		/// <summary>Parents this WorldUI to the given gameobject and then moves it to the transforms origin.</summary>
		/// <param name="parent">The transform to parent to.</param>
		public void ParentToOrigin(Transform parent){
			if(parent==null){
				transform.parent=null;
				return;
			}else{
				transform.parent=parent;
				GotoLocalOrigin();
			}
		}
		
		/// <summary>Parents this WorldUI to the given gameobject and then moves it to the gameobjects origin.</summary>
		/// <param name="parent">The gameobject to parent to.</param>
		public void ParentToOrigin(GameObject parent){
			if(parent==null){
				transform.parent=null;
				return;
			}else{
				transform.parent=parent.transform;
				GotoLocalOrigin();
			}
		}
		
		/// <summary>Moves this world UI so it's at the origin of it's parents transform.</summary>
		public void GotoLocalOrigin(){
			
			if(transform==null){
				return;
			}
			
			transform.localPosition=Vector3.zero;
			transform.localRotation=Quaternion.identity;
			
		}
		
		/// <summary>Expires this WorldUI now.</summary>
		public void Expire(){
			
			bool destroy=true;
			
			if(OnExpire!=null){
				destroy=OnExpire(this);
			}
			
			if(destroy){
				Destroy();
			}
			
		}
		
		/// <summary>Cancels a pending expiry.</summary>
		public void CancelExpiry(){
			Expires=false;
		}
		
		/// <summary>Sets an expiry time for this WorldUI. It will be destroyed in this many seconds.</summary>
		public void SetExpiry(float expiry){
			
			if(expiry<=0f){
				// Insta-expire.
				Expire();
				return;
			}
			
			// It expires:
			Expires=true;
			
			// Apply the time:
			ExpiresIn=expiry;
		}
		
		
		/// <summary>Destroys this UI. Note that this also occurs if the gameobject is destroyed;
		/// Just destroying the gameobject or a parent gameObject is all that is required.</summary>
		public void Destroy(){
			
			if(Renderer==null){
				return;
			}
		
			Renderer.Destroy();
			Renderer=null;
			
			if(gameObject!=null){
				GameObject.Destroy(gameObject);
				gameObject=null;
				transform=null;
			}
			
			// Remove it from the UI update linked list:
			if(UIBefore==null){
				UI.FirstWorldUI=UIAfter;
			}else{
				UIBefore.UIAfter=UIAfter;
			}
			
			if(UIAfter==null){
				UI.LastWorldUI=UIBefore;
			}else{
				UIAfter.UIBefore=UIBefore;
			}
		}
		
	}
	
}