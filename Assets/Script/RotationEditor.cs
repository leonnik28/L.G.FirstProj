using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotationEditor : MonoBehaviour
{

    public float rotationSpeed;
    private float targetRotationAngle = 0f;
    private float currentRotationAngle = 0f;

    void AdjustRotationLeft()
    {

        targetRotationAngle += 90f;

        if (targetRotationAngle < 0f)
        {
            targetRotationAngle += 360f;
            currentRotationAngle += 360f;
        }
        else if (targetRotationAngle >= 360f)
        {
            targetRotationAngle -= 360f;
            currentRotationAngle -= 360f;
        }
        // transform.localRotation = Quaternion.Euler(0f, targetRotationAngle, 0f);
    }

    private void Update()
    {
        if (Mathf.Abs(targetRotationAngle - currentRotationAngle) > 0.0001f)
        {
            float step = rotationSpeed * Time.deltaTime;
            currentRotationAngle = Mathf.MoveTowards(currentRotationAngle, targetRotationAngle, step);
            transform.localRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
        }
    }

    void AdjustRotationRight()
    {

            targetRotationAngle = currentRotationAngle - 90f;

            if (targetRotationAngle < 0f)
            {
                targetRotationAngle += 360f;
                currentRotationAngle += 360f;
            }
            else if (targetRotationAngle >= 360f)
            {
                targetRotationAngle -= 360f;
                currentRotationAngle -= 360f;
            }

       /* float rotationTime = 1f; // время поворота в секундах
        float elapsedTime = 0f;

        while (elapsedTime < rotationTime)
        {
            float step = (rotationSpeed * Time.deltaTime) / rotationTime;
            currentRotationAngle = Mathf.MoveTowards(currentRotationAngle, targetRotationAngle, -step); // изменение знака для поворота против часовой стрелки
          //  transform.localRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
            elapsedTime += Time.deltaTime;
        }*/
    }

    public void NewRotation(int numberButton)
    {
        if ((int)targetRotationAngle == (int)currentRotationAngle)
        {
            if (numberButton == 0)
            {
                AdjustRotationLeft();
            }
            else if (numberButton == 1)
            {
                AdjustRotationRight();
            }
        }
    }
}
