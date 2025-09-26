// 文件路径: Assets/Scripts/IKG1.cs (V4 - 最终升级版: 增加旋转控制)
using UnityEngine;
using UnityEngine.InputSystem;

public class IKG1 : MonoBehaviour
{
    [Header("IK 目标和身体")]
    [Tooltip("机器人根物体/身体的Transform，用于坐标系转换")]
    public Transform body;
    [Tooltip("左手IK目标的Transform")]
    public Transform hand1;
    [Tooltip("右手IK目标的Transform")]
    public Transform hand2;

    [Header("VR 控制设置")]
    [Tooltip("场景中代表左手柄物理位置的Transform")]
    public Transform leftControllerTransform;
    [Tooltip("场景中代表右手柄物理位置的Transform")]
    public Transform rightControllerTransform;
    [Tooltip("手臂移动灵敏度 (建议值 0.3 ~ 0.7)")]
    public float handspeed = 0.5f;
    [SerializeField] private InputActionProperty leftArmMoveAction;
    [SerializeField] private InputActionProperty rightArmMoveAction;

    // --- 位置控制变量 ---
    private Vector3 pos11, pos21;
    private Vector3 pos10, pos20;
    private bool isControllingLeftArm, isControllingRightArm;
    private Vector3 initialControllerLocalPos_L, initialControllerLocalPos_R;
    private Vector3 initialArmOffset_L, initialArmOffset_R;

    // --- NEW: 旋转控制变量 ---
    private Quaternion rot11, rot21;
    private Quaternion rot10, rot20;
    private Quaternion initialControllerLocalRot_L, initialControllerLocalRot_R;
    private Quaternion initialArmRotOffset_L, initialArmRotOffset_R;

    private ArticulationBody[] acts = new ArticulationBody[40];
    private int ActionNum;

    void Start()
    {
        if (body == null || leftControllerTransform == null || rightControllerTransform == null || hand1 == null || hand2 == null)
        {
            Debug.LogError("IKG1: 请在Inspector中拖拽赋值所有Transform字段!", this);
            this.enabled = false;
            return;
        }

        pos10 = body.InverseTransformPoint(hand1.position);
        pos20 = body.InverseTransformPoint(hand2.position);
        rot10 = Quaternion.Inverse(body.rotation) * hand1.rotation;
        rot20 = Quaternion.Inverse(body.rotation) * hand2.rotation;

        pos11 = Vector3.zero;
        pos21 = Vector3.zero;
        rot11 = Quaternion.identity;
        rot21 = Quaternion.identity;

        ActionNum = 0;
        ArticulationBody[] arts = this.GetComponentsInChildren<ArticulationBody>();
        for (int k = 0; k < arts.Length; k++)
        {
            if (arts[k].jointType == ArticulationJointType.RevoluteJoint)
            {
                acts[ActionNum++] = arts[k];
            }
        }
        for (int k = 0; k < 14; k++) SetJointTargetDeg(acts[k], 0);
    }

    void Update()
    {
        HandleArmControl(leftArmMoveAction, leftControllerTransform, ref isControllingLeftArm,
                         ref initialControllerLocalPos_L, ref initialControllerLocalRot_L,
                         ref initialArmOffset_L, ref initialArmRotOffset_L,
                         ref pos11, ref rot11);

        HandleArmControl(rightArmMoveAction, rightControllerTransform, ref isControllingRightArm,
                         ref initialControllerLocalPos_R, ref initialControllerLocalRot_R,
                         ref initialArmOffset_R, ref initialArmRotOffset_R,
                         ref pos21, ref rot21);

        pos11 = ClampArmOffset(pos11, false);
        pos21 = ClampArmOffset(pos21, true);
        rot11 = ClampArmRotation(rot11, false);
        rot21 = ClampArmRotation(rot21, true);

        hand1.position = body.TransformPoint(pos10 + pos11);
        hand2.position = body.TransformPoint(pos20 + pos21);
        hand1.rotation = body.rotation * rot10 * rot11;
        hand2.rotation = body.rotation * rot20 * rot21;
    }

    private void HandleArmControl(InputActionProperty moveAction, Transform controller, ref bool isControlling,
                                  ref Vector3 initialControllerLocalPos, ref Quaternion initialControllerLocalRot,
                                  ref Vector3 initialArmOffset, ref Quaternion initialArmRotOffset,
                                  ref Vector3 currentArmOffset, ref Quaternion currentArmRotOffset)
    {
        if (moveAction.action.IsPressed())
        {
            if (!isControlling)
            {
                isControlling = true;
                initialControllerLocalPos = body.InverseTransformPoint(controller.position);
                initialControllerLocalRot = Quaternion.Inverse(body.rotation) * controller.rotation;
                initialArmOffset = currentArmOffset;
                initialArmRotOffset = currentArmRotOffset;
            }

            Vector3 currentControllerLocalPos = body.InverseTransformPoint(controller.position);
            Vector3 posDelta = (currentControllerLocalPos - initialControllerLocalPos) * handspeed;
            currentArmOffset = initialArmOffset + posDelta;

            Quaternion currentControllerLocalRot = Quaternion.Inverse(body.rotation) * controller.rotation;
            Quaternion rotDelta = currentControllerLocalRot * Quaternion.Inverse(initialControllerLocalRot);
            currentArmRotOffset = initialArmRotOffset * rotDelta;
        }
        else
        {
            if (isControlling) isControlling = false;
        }
    }

    private Vector3 ClampArmOffset(Vector3 offset, bool isRightHand = false)
    {
        offset.y = Mathf.Clamp(offset.y, -0.1f, 0.4f);
        if (isRightHand) { offset.x = Mathf.Clamp(offset.x, -0.2f, 0.1f); }
        else { offset.x = Mathf.Clamp(offset.x, -0.1f, 0.2f); }
        offset.z = Mathf.Clamp(offset.z, -0.2f, 0.1f);
        return offset;
    }

    private Quaternion ClampArmRotation(Quaternion rotationOffset, bool isRightHand = false)
    {
        Vector3 euler = rotationOffset.eulerAngles;
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);

        euler.x = Mathf.Clamp(euler.x, -45f, 45f);
        euler.y = Mathf.Clamp(euler.y, -60f, 60f);
        euler.z = Mathf.Clamp(euler.z, -70f, 70f);

        return Quaternion.Euler(euler);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    public float[] GetAng()
    {
        float[] ang = new float[14];
        for (int i = 0; i < 14; i++) ang[i] = acts[i].jointPosition[0];
        return ang;
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 10f;
        drive.damping = 1f;
        drive.forceLimit = 10f;
        drive.target = x;
        joint.xDrive = drive;
    }
}
