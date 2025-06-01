// using UnityEngine;
// using System.Collections;

// public class FishingAnimator
// {
//     private FishingController controller;
//     private Vector3 floatStartPosition;
//     private Vector3 floatTargetPosition;
//     private Vector3 floatBasePosition;
    
//     public FishingAnimator(FishingController controller)
//     {
//         this.controller = controller;
//     }
    
//     public void InitializeVisuals()
//     {
//         SetupFloatStartPosition();
//         HideFloat();
//     }
    
//     private void SetupFloatStartPosition()
//     {
//         if (controller.floatObject != null)
//         {
//             floatStartPosition = controller.shore != null ? 
//                 controller.shore.position : controller.transform.position;
//             controller.floatObject.transform.position = floatStartPosition;
//         }
//     }
    
    
//     public IEnumerator CastAnimation()
//     {
//         if (controller.floatObject == null || controller.waterSurface == null) yield break;
        
//         ShowFloat();
//         CalculateCastTarget();
        
//         yield return controller.StartCoroutine(AnimateCastArc());
        
//         SetFloatAtTarget();
//         PlaySplashEffect();
//     }
    
    
//     private void CalculateCastTarget()
//     {
//         Vector3 castPosition = controller.waterSurface.position + 
//                               Vector3.right * controller.castDistance;
//         floatTargetPosition = castPosition;
//         floatBasePosition = castPosition;
//     }
    
//     private IEnumerator AnimateCastArc()
//     {
//         float castTime = 1.5f;
//         float elapsed = 0f;
        
//         while (elapsed < castTime)
//         {
//             elapsed += Time.deltaTime;
//             float progress = elapsed / castTime;
//             float curveValue = controller.castCurve.Evaluate(progress);
            
//             Vector3 currentPos = Vector3.Lerp(floatStartPosition, floatTargetPosition, curveValue);
//             currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * 2f;
            
//             controller.floatObject.transform.position = currentPos;
//             yield return null;
//         }
//     }
    
//     private void SetFloatAtTarget()
//     {
//         controller.floatObject.transform.position = floatTargetPosition;
//         floatBasePosition = floatTargetPosition;
//     }
    
    
    
//     public Vector3 FloatStartPosition => floatStartPosition;
//     public Vector3 FloatTargetPosition => floatTargetPosition;
//     public Vector3 FloatBasePosition => floatBasePosition;
// }