# Pivot Editor

- UPM(Unity Package Manager) 형태로 작성

<br>

# How To Import

`[Window]` - `[Package Manager]` - 좌측 상단 `[+]` - `[Add package from git URL]` - `https://github.com/rito15/Unity-Pivot-Editor.git`

![2021_0520_HowToUseUPM](https://user-images.githubusercontent.com/42164422/118945484-7425de00-b990-11eb-93d6-17853a4836c6.gif)

<br>

# Summary

- 메시의 피벗 위치를 직접 수정할 수 있는 기능을 제공합니다.
- 메시의 위치, 회전, 크기를 직접 변경할 수 있습니다.

<br>

# Preview

## **Inspector**
![image](https://user-images.githubusercontent.com/42164422/118937470-8bf96400-b988-11eb-8683-3322d51f272a.png)

## **Scene**

![image](https://user-images.githubusercontent.com/42164422/118936749-c7dff980-b987-11eb-838d-8a673497ed17.png)

<br>

# Preview(GIF)

![2021_0520_PivotEditor](https://user-images.githubusercontent.com/42164422/118942640-e21cd600-b98d-11eb-97f2-25dac89a6ab9.gif)

![2021_0520_PivotEditor2](https://user-images.githubusercontent.com/42164422/118942646-e34e0300-b98d-11eb-8dd4-72a368d8016e.gif)

<br>

# How To Use

## **Pivot Editor 컴포넌트 추가**

- `Mesh Filter` 컴포넌트를 우클릭하고 `Edit Pivot`을 클릭합니다.

- 게임오브젝트에 `Mesh Filter`, `Mesh Renderer` 컴포넌트가 모두 존재해야 합니다.

![image](https://user-images.githubusercontent.com/42164422/118944426-83f0f280-b98f-11eb-8201-fd5dbc0c330b.png)

<br>

## **Edit/Cancel 버튼**

- 피벗 수정 기능을 활성화/비활성화합니다.

<br>

## **Options**

|이름|설명|
|---|---|
|`Hide Transform Tool`|씬뷰에서 트랜스폼 도구 비활성화|
|`Edit Pivot`|씬뷰에서 피벗 위치 이동 도구 활성화|
|`Pivot Position`|새로운 피벗 위치 지정|
|`Snap`|피벗 위치를 지정한 단위로 끊어서 지정|

<br>

## **Bounds**

|이름|설명|
|---|---|
|`Show Bounds`|메시의 영역을 직육면체로 씬뷰에 표시|
|`Confine Pivot In Bounds`|메시 영역 내에서만 피벗을 지정하도록 설정|
|`X`, `Y`, `Z`|각각의 축마다 0 ~ 1 비율 내에서 피벗 위치 조정|

<br>

## **Set Pivot**

|이름|설명|
|---|---|
|`Reset`|수정 이전의 피벗 위치로 복귀|
|`Bottom Center`|메시 영역 내에서 XZ평면 중앙, Y축 하단으로 피벗 설정|
|`Center`|메시 영역 내에서 중앙 지점으로 피벗 설정|
|`Top Center`|메시 영역 내에서 XZ평면 중앙, Y축 상단으로 피벗 설정|

<br>

## **Reset Transform**

|이름|설명|
|---|---|
|`All`|위치, 회전, 크기를 모두 기본값으로 지정|
|`Position`|로컬 위치 값을 (0, 0, 0)으로 지정|
|`Rotation`|로컬 회전 값을 (0, 0, 0)으로 지정|
|`Scale`|로컬 크기 값을(1, 1, 1)로 지정|

<br>

## **Save**

|이름|설명|
|---|---|
|`Mesh Name`|새롭게 지정할 메시 이름|
|`Apply`|수정된 결과를 현재 메시 필터에 적용|
|`Save As Obj File`|수정된  Obj 파일로 추출하여 저장|

<br>

<br>

# Contributors

<a href="https://github.com/Hdongyeop/">
  <img src="https://contrib.rocks/image?repo=Hdongyeop/Hdongyeop" />
</a>
<!-- Made with [contrib.rocks](https://contrib.rocks). -->
