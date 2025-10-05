using UnityEngine;

namespace NasaSpaceApps.FarmFromSpace.Game
{
	public class FarmCameraController : MonoBehaviour
	{
		[Header("Camera Settings")]
		[SerializeField] private float panSpeed = 5f;
		[SerializeField] private float zoomSpeed = 2f;
		[SerializeField] private float smoothTime = 0.3f;
		
		[Header("Bounds")]
		[SerializeField] private Vector2 minBounds = new Vector2(-10f, -10f);
		[SerializeField] private Vector2 maxBounds = new Vector2(10f, 10f);
		[SerializeField] private float minZoom = 3f;
		[SerializeField] private float maxZoom = 10f;

		[Header("Mobile Settings")]
		[SerializeField] private float touchPanSensitivity = 1f;
		[SerializeField] private float touchZoomSensitivity = 0.5f;
		[SerializeField] private float pinchZoomThreshold = 0.1f;

		private UnityEngine.Camera cam;
		private Vector3 targetPosition;
		private float targetZoom;
		private Vector3 velocity = Vector3.zero;
		private Vector3 lastPanPosition;
		private float lastZoomDistance;

		private void Start()
		{
			cam = GetComponent<UnityEngine.Camera>();
			if (cam == null)
			{
				Debug.LogError("FarmCameraController: No Camera component found!");
				return;
			}

			targetPosition = transform.position;
			targetZoom = cam.orthographicSize;
		}

		private void Update()
		{
			HandleInput();
			SmoothMovement();
		}

		private void HandleInput()
		{
			// PC Controls
			HandleMouseInput();
			
			// Mobile Controls
			HandleTouchInput();
			
			// Keyboard Controls (optional)
			HandleKeyboardInput();
		}

		private void HandleMouseInput()
		{
			// Right-click and drag to pan
			if (Input.GetMouseButtonDown(1))
			{
				lastPanPosition = cam.ScreenToWorldPoint(Input.mousePosition);
			}
			else if (Input.GetMouseButton(1))
			{
				Vector3 currentPanPosition = cam.ScreenToWorldPoint(Input.mousePosition);
				Vector3 panDelta = lastPanPosition - currentPanPosition;
				targetPosition += panDelta;
				lastPanPosition = currentPanPosition;
			}

			// Mouse wheel zoom
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			if (scroll != 0f)
			{
				targetZoom -= scroll * zoomSpeed;
			}
		}

		private void HandleTouchInput()
		{
			if (Input.touchCount == 1)
			{
				// Single touch - pan
				Touch touch = Input.GetTouch(0);
				
				if (touch.phase == TouchPhase.Began)
				{
					lastPanPosition = cam.ScreenToWorldPoint(touch.position);
				}
				else if (touch.phase == TouchPhase.Moved)
				{
					Vector3 currentPanPosition = cam.ScreenToWorldPoint(touch.position);
					Vector3 panDelta = lastPanPosition - currentPanPosition;
					targetPosition += panDelta * touchPanSensitivity;
					lastPanPosition = currentPanPosition;
				}
			}
			else if (Input.touchCount == 2)
			{
				// Two touches - pinch zoom
				Touch touch1 = Input.GetTouch(0);
				Touch touch2 = Input.GetTouch(1);
				
				float currentDistance = Vector2.Distance(touch1.position, touch2.position);
				
				if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
				{
					lastZoomDistance = currentDistance;
				}
				else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
				{
					float deltaDistance = currentDistance - lastZoomDistance;
					if (Mathf.Abs(deltaDistance) > pinchZoomThreshold)
					{
						targetZoom -= deltaDistance * touchZoomSensitivity;
						lastZoomDistance = currentDistance;
					}
				}
			}
		}

		private void HandleKeyboardInput()
		{
			// WASD or Arrow keys for panning
			Vector3 keyboardInput = Vector3.zero;
			
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
				keyboardInput.y += 1f;
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
				keyboardInput.y -= 1f;
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
				keyboardInput.x -= 1f;
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
				keyboardInput.x += 1f;

			if (keyboardInput != Vector3.zero)
			{
				targetPosition += keyboardInput * panSpeed * Time.deltaTime;
			}

			// +/- keys for zoom
			if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals))
				targetZoom -= zoomSpeed * Time.deltaTime;
			if (Input.GetKey(KeyCode.Minus))
				targetZoom += zoomSpeed * Time.deltaTime;
		}

		private void SmoothMovement()
		{
			// Apply bounds
			targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
			targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
			targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

			// Smooth position
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
			
			// Smooth zoom
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * (1f / smoothTime));
		}

		// Public methods for external control
		public void SetTargetPosition(Vector3 position)
		{
			targetPosition = position;
		}

		public void SetTargetZoom(float zoom)
		{
			targetZoom = zoom;
		}

		public void FocusOnPlot(Vector3 plotPosition, float zoom = 5f)
		{
			targetPosition = plotPosition;
			targetZoom = zoom;
		}

		// Debug methods
		[ContextMenu("Reset Camera")]
		public void ResetCamera()
		{
			targetPosition = Vector3.zero;
			targetZoom = 5f;
		}
	}
}
