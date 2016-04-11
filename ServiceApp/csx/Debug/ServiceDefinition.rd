<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="ServiceApp" generation="1" functional="0" release="0" Id="91866a88-d501-44ce-ac9f-550570da29b0" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="ServiceAppGroup" generation="1" functional="0" release="0">
      <settings>
        <aCS name="SBQWorker:Microsoft.Database.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/ServiceApp/ServiceAppGroup/MapSBQWorker:Microsoft.Database.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="SBQWorker:Microsoft.ServiceBus.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/ServiceApp/ServiceAppGroup/MapSBQWorker:Microsoft.ServiceBus.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="SBQWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/ServiceApp/ServiceAppGroup/MapSBQWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="SBQWorker:StorageConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/ServiceApp/ServiceAppGroup/MapSBQWorker:StorageConnectionString" />
          </maps>
        </aCS>
        <aCS name="SBQWorkerInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/ServiceApp/ServiceAppGroup/MapSBQWorkerInstances" />
          </maps>
        </aCS>
      </settings>
      <maps>
        <map name="MapSBQWorker:Microsoft.Database.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/ServiceApp/ServiceAppGroup/SBQWorker/Microsoft.Database.ConnectionString" />
          </setting>
        </map>
        <map name="MapSBQWorker:Microsoft.ServiceBus.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/ServiceApp/ServiceAppGroup/SBQWorker/Microsoft.ServiceBus.ConnectionString" />
          </setting>
        </map>
        <map name="MapSBQWorker:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/ServiceApp/ServiceAppGroup/SBQWorker/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapSBQWorker:StorageConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/ServiceApp/ServiceAppGroup/SBQWorker/StorageConnectionString" />
          </setting>
        </map>
        <map name="MapSBQWorkerInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/ServiceApp/ServiceAppGroup/SBQWorkerInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="SBQWorker" generation="1" functional="0" release="0" software="C:\Users\Grayden\documents\visual studio 2015\Projects\ServiceApp\ServiceApp\csx\Debug\roles\SBQWorker" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="-1" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="Microsoft.Database.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.ServiceBus.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="StorageConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;SBQWorker&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;SBQWorker&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/ServiceApp/ServiceAppGroup/SBQWorkerInstances" />
            <sCSPolicyUpdateDomainMoniker name="/ServiceApp/ServiceAppGroup/SBQWorkerUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/ServiceApp/ServiceAppGroup/SBQWorkerFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="SBQWorkerUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="SBQWorkerFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="SBQWorkerInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
</serviceModel>