using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public new Camera camera;
    public Transform player;
    public new Rigidbody rigidbody;
    public Transform skies;
    public LayerMask groundMask;
    public float groundCheckSphereYOffset = -1;

    public float sensivity = 0.5f;
    private float mouseX = 0;
    private float mouseY = 0;

    public float speed = 5f;
    public float jumpHeight = 10;
    [Space]
    public float camBendMaxAngle = 10;
    public ParticleSystem leftSkiSlideParticle, rightSkiSlideParticle;
    public ParticleSystem speedParticle;

    public bool IsGrounded => isGrounded;
    public bool IsHighGrounded => isHighGrounded;
    public bool IsMoving => isMoving;

    private bool isMoving = false;
    private bool isGrounded = false;
    private bool isHighGrounded = false;
    private Vector3 camInitialLocalPos;
    private float camZRot = 0.0f;
    private float camZBend = 0.0f;
    private Vector3 bodyUpDir, bodyRightDir, bodyFrontDir;
    private Vector3 groundCheckSpherePos;
    private Vector3 cameraFallShakeOffset = Vector3.zero;
    private bool rightParticlePlaying = false;
    private bool leftParticlePlaying = false;
    private float maxVelocity = float.MinValue;
    private float minVelocity = float.MaxValue;
    private float fallFOV = 0.0f;

    private void OnValidate()
    {
        this.camera = GetComponentInChildren<Camera>();
        this.player = GetComponent<Transform>();   
        this.rigidbody = GetComponent<Rigidbody>(); 
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camInitialLocalPos = camera.transform.localPosition;
    }

    void Update() 
    {
        float deltaTime = Time.deltaTime;

        //mouseX += Input.GetAxis("Mouse X") * sensivity;
        //mouseY += Input.GetAxis("Mouse Y") * sensivity;
        //mouseY = Mathf.Clamp(mouseY, -80f, 80f);
        mouseY = -20.0f;
        mouseX = Input.GetAxis("Horizontal") * 10.0f;

        groundCheckSpherePos = transform.position + Vector3.up * groundCheckSphereYOffset;
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheckSpherePos, 1f, groundMask);
        isHighGrounded = Physics.CheckSphere(groundCheckSpherePos, 2.5f, groundMask);

        if (isGrounded && wasGrounded != isGrounded)
            cameraFallShakeOffset = Vector3.down * 0.75f;
        cameraFallShakeOffset = Vector3.Lerp(cameraFallShakeOffset, Vector3.zero, deltaTime * 5f);

        if (isMoving && isGrounded) {
            Vector3 camLocalPos = camera.transform.localPosition;
            float time = Time.timeSinceLevelLoad * speed * 3.5f;
            camLocalPos.y = camInitialLocalPos.y + Mathf.Sin(time) * 0.05f;
            camZRot = camInitialLocalPos.z + Mathf.Cos(time / 2.0f) * 3.0f;
            camera.transform.localPosition = camLocalPos + cameraFallShakeOffset;
        }
        else {
            camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, camInitialLocalPos + cameraFallShakeOffset, deltaTime * 2.0f);
            camZRot = Mathf.Lerp(camZRot, 0, deltaTime * 2.0f);
        }

        camZBend = Mathf.Lerp(camZBend, camBendMaxAngle * horizontalInputAxis, deltaTime * 5f);

        Quaternion cameraRotation = Quaternion.Euler(-mouseY, mouseX, camZRot - camZBend);
        camera.transform.rotation = cameraRotation;
        Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, 1000f, groundMask);
        Vector3 normalRight = Vector3.Cross(-Vector3.forward, hit.normal).normalized;
        if (normalRight == Vector3.zero)
            normalRight = Quaternion.Euler(0, mouseX, 0) * Vector3.Cross(camera.transform.forward, hit.normal);
        Vector3 normalForward = Vector3.Cross(normalRight, hit.normal).normalized;

        Quaternion localRotation = Quaternion.AngleAxis(mouseX + camZBend * 3.5f, hit.normal);

        bodyUpDir = hit.normal;
        bodyFrontDir = localRotation * normalForward;
        bodyRightDir = localRotation * normalRight;
        skies.rotation = Quaternion.Slerp(skies.rotation, Quaternion.LookRotation(bodyRightDir, bodyUpDir), deltaTime * 4.0f);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space)) {
            rigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
#endif

        float velocity = rigidbody.velocity.magnitude;
        float velocityShrink = deltaTime / 5.0f;
        maxVelocity -= velocityShrink * 4.0f;
        minVelocity += velocityShrink;
        if (velocity > maxVelocity && isGrounded)
            maxVelocity = velocity;
        if (velocity < minVelocity)
            minVelocity = velocity;
        float velocityTime = (velocity - minVelocity) / (maxVelocity - minVelocity);

        handleFOV(deltaTime, velocityTime);
        handleParticleEffects(velocity);
    }

    private void handleFOV(float deltaTime, float velocityTime) {
        const float minFOV = 80f;
        const float maxFallFOV = 40f;
        if (isMoving) {
            float velocityFOV = Mathf.Lerp(0f, 10f, velocityTime);
            if (!isGrounded) {
                fallFOV += deltaTime * 20.0f;
                if (fallFOV >= maxFallFOV)
                    fallFOV = maxFallFOV;
            }
            else 
                fallFOV = 0.0f;
            
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, minFOV + velocityFOV + fallFOV, deltaTime * speed);
        }
        else {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, minFOV, deltaTime * speed);
        }
    }

    private void handleParticleEffects(float velocity) {
        bool isFast = velocity > 30f,
            isVeryFast = velocity > 45f;
        if (isGrounded && horizontalInputAxis < 0 && isFast && !rightParticlePlaying) { 
            rightSkiSlideParticle.Play();
            rightParticlePlaying = true;
        }
        else if ((!isGrounded || horizontalInputAxis >= 0 || !isFast) && rightParticlePlaying ) {
            rightSkiSlideParticle.Stop();
            rightParticlePlaying = false;
        }

        if (isGrounded && horizontalInputAxis > 0  && isFast && !leftParticlePlaying) {
            leftSkiSlideParticle.Play();
            leftParticlePlaying = true;
        }
        else if ((!isGrounded || horizontalInputAxis <= 0 || !isFast) && leftParticlePlaying) {
            leftSkiSlideParticle.Stop();
            leftParticlePlaying = false;
        }

        bool speedParticleEnabled = isVeryFast;
        if (!speedParticle.isPlaying && speedParticleEnabled)
            speedParticle.Play();
        else if (speedParticle.isPlaying && !speedParticleEnabled)
            speedParticle.Stop();
    }

    float horizontalInputAxis = 0;
    void FixedUpdate() {
        Vector3 forward = Quaternion.Euler(0, mouseX, 0) * Vector3.forward;
        Vector3 right = Vector3.Cross(forward, Vector3.down);
        Vector3 inputDirection = Vector3.zero;

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_EDITOR
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.position.x < Screen.width / 2)
                horizontalInputAxis = -1;
            else
                horizontalInputAxis = 1;
        }
        else 
            horizontalInputAxis = Input.GetAxisRaw("Horizontal");
#endif
        float groundControl = isGrounded ? 8f : 1.0f;
        inputDirection += right * horizontalInputAxis * groundControl;
        inputDirection += forward; // * Input.GetAxisRaw("Vertical");
        if (isGrounded && horizontalInputAxis != 0)
            inputDirection += Vector3.back * 0.1f;

        inputDirection = inputDirection.normalized * speed;

        if (inputDirection.magnitude > 0) {
            rigidbody.AddForce(inputDirection * speed, ForceMode.Acceleration);
            isMoving = true;
        }
        else {
            isMoving = false;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(player.position, bodyUpDir);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(player.position, bodyRightDir);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(player.position, bodyFrontDir);
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawSphere(groundCheckSpherePos, 1f);
    }
}
