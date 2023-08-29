using UnityEngine;
using System;



/**
 * 작성자: 20181220 이성수
 * 에디터에서 사용 가능한 커스텀 어트리뷰트입니다.
 * 사용법은 변수 앞에 [ReadOnlyProperty] 또는 [ReadOnlyProperty(bool 값)] 입니다.
 * 기본적으로 에디터에서 편집 불가능한 채 보여주기만 하는 속성을 정의하고자 할 때 사용합니다.
 * bool 값에 따라, 참이라면 게임 실행 중에만 수정 불가한 속성이 됩니다. 반대인 경우 항상 읽기 전용이 됩니다.
 * public 접근 지정자를 가졌거나 [SerializeField] 에 의해 에디터에 노출된 상태에서만 작동합니다.
 * 특정 변수를 에디터에서 읽기 전용으로 사용하기 위해 존재합니다.
 * 언리얼 엔진으로 비유하면, VisibleAnywhere 프로퍼티와 유사합니다.
 */
[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyProperty : PropertyAttribute
{
	public readonly bool RuntimeOnly;

	/// <summary>
	/// 필드를 읽기 전용으로 만들어 에디터에서 편집할 수 없도록 만듭니다.
	/// </summary>
	/// <param name="RuntimeOnly">true 인 경우, 게임 실행 중에만 수정할 수 없습니다.</param>
	public ReadOnlyProperty(bool RuntimeOnly = false)
	{
		this.RuntimeOnly = RuntimeOnly;
	}
}



#if UNITY_EDITOR
namespace UnityEditor
{
	[CustomPropertyDrawer(typeof(ReadOnlyProperty))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}
#endif