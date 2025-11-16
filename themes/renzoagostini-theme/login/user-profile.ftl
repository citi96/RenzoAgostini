<#macro renderAttributes attributes>
    <#list attributes as attribute>
        <#assign annotations = attribute.annotations!{}>
        <#assign fieldName = attribute.name>
        <#assign fieldId = fieldName?replace('.', '-')>
        <#assign fieldLabel = attribute.displayName!fieldName>
        <#assign readOnly = attribute.readOnly!false>
        <#assign required = attribute.required!false>
        <#assign inputType = annotations['inputType']!"text">
        <#assign options = annotations['options']!"">
        <#assign currentValues = attribute.values![]>
        <#assign value = (currentValues?has_content)?then(currentValues[0], attribute.value!'')>
        <div class="kc-field <#if required>required</#if>">
            <label for="${fieldId}">${fieldLabel}<#if required>*</#if></label>
            <#if inputType == 'textarea'>
                <textarea id="${fieldId}" name="${fieldName}" rows="3" <#if readOnly>readonly</#if>>${value?html}</textarea>
            <#elseif inputType == 'select' && options?has_content>
                <select id="${fieldId}" name="${fieldName}" <#if readOnly>disabled</#if>>
                    <#list options?split(',') as option>
                        <#assign optionTrim = option?trim>
                        <option value="${optionTrim?html}" <#if value?trim == optionTrim>selected</#if>>${optionTrim?html}</option>
                    </#list>
                </select>
            <#else>
                <input id="${fieldId}" name="${fieldName}" type="${inputType}" value="${value?html}" <#if readOnly>readonly</#if> />
            </#if>
            <@layout.fieldError fieldName=fieldName />
        </div>
    </#list>
</#macro>
