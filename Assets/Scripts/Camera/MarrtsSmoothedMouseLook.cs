using UnityEngine;

//Marrt's simplest Mouselook for https://forum.unity.com/threads/a-free-simple-smooth-mouselook.73117/page-2#post-4652755

namespace UnityLibrary
{
  public class MarrtsSmoothedMouseLook : MonoBehaviour 
  {
    [Header("CameraTransform")]
    public	Transform	targetTrans;
    [Header("On/Off & Settings")]
    public	bool		inputActive		= true;
    public	bool		controlCursor	= false;
    [Header("Smoothing")]
    public	bool		byPassSmoothing	= false;
    public	float		mLambda			= 20F;	//higher = less latency but also less smoothing
    [Header("Sensitivity")]
    public	float		hSens			=  4F;
    public	float		vSens			=  4F;
    public	BufferV2	mouseBuffer		= new BufferV2();

    public	enum AxisClampMode	{ None, Hard, Soft }

    [Header("Restricting Look")]

    public	AxisClampMode	pitchClamp		= AxisClampMode.Soft;
    [Range(-180F,180F)]	public	float		pMin			= -80F;
    [Range(-180F,180F)]	public	float		pMax			=  80F;


    [Header("Yaw should be left None, Message me if you really need this functionality")]

    public	AxisClampMode	yawClamp		= AxisClampMode.None;
    [Range(-180F,180F)]	public	float		yMin			= -140F;
    [Range(-180F,180F)]	public	float		yMax			=  140F;



    //public	bool		smoothingDependence	= Timescale Framerate

    void Update ()
    {
      //if(Input.GetKeyDown(KeyCode.Space)){inputActive = !inputActive;}
      if(controlCursor){	//Cursor Control
        if(  inputActive && Cursor.lockState != CursorLockMode.Locked)	{ Cursor.lockState = CursorLockMode.Locked;	}
        if( !inputActive && Cursor.lockState != CursorLockMode.None)	{ Cursor.lockState = CursorLockMode.None;	}
      }		
      if(!inputActive){ return; }	//active?

      //Update input
      UpdateMouseBuffer();
      targetTrans.rotation		= Quaternion.Euler( mouseBuffer.curAbs );
    }

    //consider late Update for applying the rotation if your game needs it (e.g. if camera parents are rotated in Update for some reason)
    void LateUpdate() {}	

    private	void UpdateMouseBuffer()
    {

      float rawPitchDelta = vSens * -Input.GetAxisRaw("Mouse Y");

      switch(pitchClamp){
        case AxisClampMode.None:	mouseBuffer.target.x += rawPitchDelta;	break;
        case AxisClampMode.Hard:	mouseBuffer.target.x  = Mathf.Clamp(mouseBuffer.target.x +rawPitchDelta, pMin, pMax);		break;
        case AxisClampMode.Soft:	mouseBuffer.target.x += SoftPitchClamping.DeltaMod( mouseBuffer.target.x, rawPitchDelta, Mathf.Abs(pMax*0.5F), Mathf.Abs(pMax) );	break; //symetric clamping only for now, max is used
      }

      float rawYawDelta = hSens * Input.GetAxisRaw("Mouse X");

      switch(yawClamp){
        case AxisClampMode.None:	mouseBuffer.target.y += rawYawDelta;	break;
        case AxisClampMode.Hard:	mouseBuffer.target.y  = Mathf.Clamp(mouseBuffer.target.y +rawYawDelta, yMin, yMax);			break;
        case AxisClampMode.Soft:	Debug.LogWarning("SoftYaw clamp should be implemented with Quaternions to work in every situation");
                      mouseBuffer.target.y += SoftPitchClamping.DeltaMod( mouseBuffer.target.y, rawYawDelta, Mathf.Abs(yMax*0.5F), Mathf.Abs(yMax) );
        break;
      }

      mouseBuffer.Update( mLambda, Time.deltaTime, byPassSmoothing );
    }
  }





  #region Helpers
  [System.Serializable]
  public class BufferV2{

    public BufferV2(){
      this.target = Vector2.zero;
      this.buffer = Vector2.zero;
    }
    public BufferV2( Vector2 targetInit, Vector2 bufferInit ) {
      this.target = targetInit;
      this.buffer = bufferInit;
    }

    /*private*/public	Vector2	target;
    /*private*/public	Vector2	buffer;

    public	Vector2	curDelta;	//Delta: apply difference from lastBuffer state to current BufferState		//get difference between last and new buffer
    public	Vector2	curAbs;		//absolute



    /// <summary>Update Buffer By supplying new target</summary>
    public	void UpdateByNewTarget( Vector2 newTarget, float dampLambda, float deltaTime ){
      this.target		= newTarget;
      Update(dampLambda, deltaTime);
    }
    /// <summary>Update Buffer By supplying the rawDelta to the last target</summary>
    public	void UpdateByDelta( Vector2 rawDelta, float dampLambda, float deltaTime ){
      this.target		= this.target +rawDelta;	//update Target
      Update(dampLambda, deltaTime);
    }

    /// <summary>Update Buffer</summary>
    public	void Update( float dampLambda, float deltaTime, bool byPass = false ){
      Vector2 last	= buffer;			//last state of Buffer
      this.buffer		= byPass? target : DampToTargetLambda( buffer, this.target, dampLambda, deltaTime);	//damp current to target
      this.curDelta	= buffer -last;
      this.curAbs		= buffer;
    }
    public static Vector2		DampToTargetLambda( Vector2	current,		Vector2		target,		float lambda, float dt){
      return Vector2.		Lerp(current, target, 1F -Mathf.Exp( -lambda *dt) );
    }
  }




  public	static class SoftPitchClamping{
    public	static float	DeltaMod( float currentPitch, float delta, float softMax = 45F, float hardMax = 85F ){

        //doesnt work for input above 90F pitch, unity might invert roll and decrease pitch again

        //transform into -180 to 180 range (allowed input range = 0-360F )
        float wrapped		= Wrap.Float( currentPitch, -180F, 180F );

        float sign			= Mathf.Sign( wrapped );
        float absolute		= Mathf.Abs	( wrapped );

        //	treat current as mapped value, so unmap it via reversing
        //	https://rechneronline.de/function-graphs/
        //	revert remap:	e^((((log(x/45)+1)*45)/45)-1)*45
        //	remap:	(log(x/45)+1)*45

        float remapped			= absolute;
        if( absolute > softMax ){
          //				 e^ ((			((		  log(       x/45	  )+1  )*45		 )		/45		) -1 )*45
        //	remapped = Mathf.Exp((			((	Mathf.Log(remapped/softMax)+1F )*softMax )		/softMax) -1F)*softMax ;
          remapped = Mathf.Exp((			remapped											/softMax) -1F)*softMax ;
          //x*0.5+45*0.5
        }

        //apply delta to unmapped, sign needs to be taken into consideration for delta
        remapped += (delta *sign);	//float raw = remapped +(delta *sign);

        //remap, only needed if exceeding softrange
        if( remapped > softMax ){
          //								((		 log(		 x/45	 )+1  )*45		)
          remapped =						(( Mathf.Log(remapped/softMax)+1F )*softMax );

          //x*0.5+45*0.5

        }

        remapped *= sign;
        remapped  = Mathf.Clamp( remapped, -hardMax, +hardMax);

        float newDelta = ( remapped -wrapped );

        //print( "wrapped\t"+wrapped+" (from:"+currentPitch+")"+"\nremapped\t"+remapped +" (raw :"+raw+")");

        return newDelta;
      //	return delta;
    }

    public	static class Wrap{

      //can be used to clamp angles from 0-360 to 0-180 by feeding (value,-180,180)
      //https://stackoverflow.com/questions/1628386/normalise-orientation-between-0-and-360

      //Normalizes any number to an arbitrary range 
      //by assuming the range wraps around when going below min or above max 
      public	static float Float( float value, float start, float end ){
        float width       = end - start   ;   // 
        float offsetValue = value - start ;   // value relative to 0

        return ( offsetValue - ( Mathf.Floor( offsetValue / width ) * width ) ) + start ;
        // + start to reset back to start of original range
      }

      //Normalizes any number to an arbitrary range 
      //by assuming the range wraps around when going below min or above max 
      public	static int Int( int value, int start, int end ){
        int width       = end - start   ;   // 
        int offsetValue = value - start ;   // value relative to 0

        return ( offsetValue - ( ( offsetValue / width ) * width ) ) + start ;
        // + start to reset back to start of original range
    }
  }
  }
    #endregion Helpers
}
