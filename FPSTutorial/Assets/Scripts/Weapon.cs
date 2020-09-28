using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour {

    //Weapon Specification
    public string weaponName;
    public int bulletsPerMag;
    public int bulletsTotal;
    public int currentBullets;
    public float range;
    public float fireRate;

    public Vector3 aimPosition;
    private Vector3 originalPosition;

    //Recoil
    public Transform camRecoil;
    public Vector3 recoilKickback;
    public float recoilAmount;

    //Parameters
    private float fireTimer;
    private bool isReloading;
    private bool isAiming;
	private bool isRunning;

    //References
    public Transform shootPoint;
    private Animator anim;
    public ParticleSystem muzzleFlash;
    public Text bulletsText;
	public Transform bulletCasingPoint;
	private CharacterController characterController;

    //Prefabs
    public GameObject hitSparkPrefabs;
    public GameObject hitHolePrefabs;
	public GameObject bulletCasing;

    //Sounds
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

	//Weapon Specification
	public float accuracy;
	private float originalAccuracy;
	public float damage;

	//Recoil
	private float originalRecoil;

	// Use this for initialization
	void Start () {
        originalPosition = transform.localPosition;
		originalAccuracy = accuracy;
		originalRecoil = recoilAmount;
        currentBullets = bulletsPerMag;
        anim = GetComponent<Animator>();
        bulletsText.text = currentBullets + " / " + bulletsTotal;
		characterController = GetComponentInParent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");

        if (Input.GetButton("Fire1")){
            if (currentBullets > 0)
                Fire();
            else
                DoReload();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            DoReload();
        }

        if (fireTimer < fireRate) {
            fireTimer += Time.deltaTime;
        }

        AimDownSights();
        RecoilBack();
		Run();
    }

    private void Fire() {
        if (fireTimer < fireRate || isReloading || isRunning)
            return;

        RaycastHit hit;

		//if(Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hit, range)) {
		if (Physics.Raycast(shootPoint.position, shootPoint.transform.forward + Random.onUnitSphere*accuracy, out hit, range)){

			HealthManager healthManager = hit.transform.GetComponent<HealthManager>();
			if (healthManager) {
				healthManager.ApplyDamage(damage);
			}

			Rigidbody rigidbody = hit.transform.GetComponent<Rigidbody>();
			if (rigidbody) {
				rigidbody.AddForceAtPosition(transform.forward * 5.0f * damage, transform.position);
			}

			GameObject hitSpark = Instantiate(hitSparkPrefabs, hit.point + hit.normal*0.02f, Quaternion.FromToRotation(Vector3.up, hit.normal));
			hitSpark.transform.SetParent(hit.transform);
			Destroy(hitSpark, 0.5f);

            GameObject hitHole = Instantiate(hitHolePrefabs, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
			hitHole.transform.SetParent(hit.transform);
            Destroy(hitHole, 5.0f);
        }

        currentBullets--;
        fireTimer = 0.0f;
        //audioSource.Play();
        audioSource.PlayOneShot(shootSound);
        anim.CrossFadeInFixedTime("Fire", 0.01f);
        muzzleFlash.Play();

        bulletsText.text = currentBullets + " / " + bulletsTotal;

        Recoil();
		BulletEffect();
	}

	private void BulletEffect()
	{
		Quaternion randomQuaternion = new Quaternion(Random.Range(0, 360.0f), Random.Range(0, 360.0f), Random.Range(0, 360.0f), 1);
		GameObject casing = Instantiate(bulletCasing, bulletCasingPoint);
		casing.transform.localRotation = randomQuaternion;
		casing.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(Random.Range(50.0f, 100.0f), Random.Range(50.0f, 100.0f), Random.Range(-30.0f, 30.0f)));
		Destroy(casing, 1.0f);
	}

    private void DoReload() {
        if(!isReloading && !isAiming && currentBullets < bulletsPerMag && bulletsTotal > 0) {
            anim.CrossFadeInFixedTime("Reload", 0.01f);
            audioSource.PlayOneShot(reloadSound);
        }
    }

    public void Reload() {
        int bulletsToReload = bulletsPerMag - currentBullets;
        
        if(bulletsToReload > bulletsTotal)
            bulletsToReload = bulletsTotal;

        currentBullets += bulletsToReload;
        bulletsTotal -= bulletsToReload;
        bulletsText.text = currentBullets + " / " + bulletsTotal;
    }

    private void AimDownSights(){
        if(Input.GetButton("Fire2") && !isReloading){
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * 8.0f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 40.0f, Time.deltaTime * 8.0f);
			accuracy = originalAccuracy / 2.0f;
			recoilAmount = originalRecoil / 2.0f;
            isAiming = true;
        }
        else{
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5.0f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60.0f, Time.deltaTime * 8.0f);
			accuracy = originalAccuracy;
			recoilAmount = originalRecoil;
            isAiming = false;
        }
    }

    private void Recoil(){
        Vector3 recoilVector = new Vector3(Random.Range(-recoilKickback.x, recoilKickback.x), recoilKickback.y, recoilKickback.z);
        Vector3 recoilCamVector = new Vector3(-recoilVector.y * 400.0f, recoilVector.x * 200.0f, 0.0f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + recoilVector, recoilAmount / 2.0f);
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.Euler(camRecoil.localEulerAngles + recoilCamVector), recoilAmount);
    }

    private void RecoilBack(){
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.identity, Time.deltaTime * 2.0f);
    }

	private void Run() {
		Debug.Log(characterController.velocity.sqrMagnitude);
		anim.SetBool("isRunning", Input.GetKey(KeyCode.LeftShift));
		isRunning = characterController.velocity.sqrMagnitude > 99 ? true : false;
		anim.SetFloat("Speed", characterController.velocity.sqrMagnitude);
	}
}
