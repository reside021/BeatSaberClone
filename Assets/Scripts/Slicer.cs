using UnityEngine;
using EzySlice;
using TMPro;

public class Slicer : MonoBehaviour
{
    public Transform CutPlane;
    public Material CrossMaterial;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI numberComboText;

    public static int combo;
    private double score;
    private const int point = 1;


    private void Start()
    {
        score = 0;
        combo = 1;
    }

    private void FixedUpdate()
    {
        var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);    
        transform.position = objPosition;

        

        numberComboText.text = combo.ToString();

        ScoreText.text = score.ToString();
    }


    private void OnCollisionEnter(Collision otherObj)
    {
        if (otherObj.gameObject.CompareTag("Cube"))
        {
            var angleForCut = Mathf.Atan2(transform.position.y, transform.position.x) * (180 / Mathf.PI);
            CutPlane.transform.eulerAngles = new Vector3(0, 0, angleForCut);


            SlicedHull hull = SliceObject(otherObj.gameObject, CrossMaterial);

            if (hull != null)
            {
                GameObject bottom = hull.CreateLowerHull(otherObj.gameObject, CrossMaterial);
                GameObject top = hull.CreateUpperHull(otherObj.gameObject, CrossMaterial);
                AddHullComponents(bottom);
                AddHullComponents(top);
                Destroy(otherObj.gameObject);
                AddScoreAndCombo();
            }


        }
    }

    private void AddScoreAndCombo()
    {
        score += point * combo;
        if (score % 5 == 0) 
            combo++;
    }

    private void AddHullComponents(GameObject go)
    {
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;
        go.AddComponent<Destroyer>();
        rb.AddExplosionForce(100, go.transform.position, 20);
        rb.AddForce(Vector3.back * 3, ForceMode.Impulse);
    }

    private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        if (obj.GetComponent<MeshFilter>() == null)
            return null;

        return obj.Slice(CutPlane.position, CutPlane.up, crossSectionMaterial);
    }

}
