using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour
{

     public enum Type
    {
        NONE,
        RED,
        GREEN,
        BLUE,
        YELLOW
    }

    public Type type;

    public Material blueMaterial;
    public Material redMaterial;
    public Material yellowMaterial;
    public Material greenMaterial;

    public bool isSelected = false;

    public int deleteCount = 0;

    public int deleteCountLimit = 0;

    public bool isDeleting = false;

    private new Renderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (type)
        {
            case Type.NONE:
                renderer.material.color = new Color(0, 0, 0, 0.0f);
                break;
            case Type.RED:
                renderer.material = this.redMaterial;
                break;
            case Type.BLUE:
                renderer.material = this.blueMaterial;
                break;
            case Type.GREEN:
                renderer.material = this.greenMaterial;
                break;
            case Type.YELLOW:
                renderer.material = this.yellowMaterial;
                break;
        }

        if (type != Type.NONE)
        {
            if (isSelected)
            {
                renderer.material.color = new Color(1.0f, 0.5f, 0.5f, 1);
            }
            else
            {
                renderer.material.color = new Color(1, 1, 1, 1);
            }
        }


        if (isDeleting)
        {
            renderer.material.color = new Color(0, 0, 1.0f);
            ++deleteCount;
        }

        if (deleteCount > deleteCountLimit)
        {
            type = Type.NONE;
            deleteCount = 0;
            isDeleting = false;
        }

    }
}
