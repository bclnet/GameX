
#include <AzCore/Serialization/SerializeContext.h>

#include "Shell3DSystemComponent.h"

#include <Shell3D/Shell3DTypeIds.h>

namespace Shell3D
{
    AZ_COMPONENT_IMPL(Shell3DSystemComponent, "Shell3DSystemComponent",
        Shell3DSystemComponentTypeId);

    void Shell3DSystemComponent::Reflect(AZ::ReflectContext* context)
    {
        if (auto serializeContext = azrtti_cast<AZ::SerializeContext*>(context))
        {
            serializeContext->Class<Shell3DSystemComponent, AZ::Component>()
                ->Version(0)
                ;
        }
    }

    void Shell3DSystemComponent::GetProvidedServices(AZ::ComponentDescriptor::DependencyArrayType& provided)
    {
        provided.push_back(AZ_CRC_CE("Shell3DService"));
    }

    void Shell3DSystemComponent::GetIncompatibleServices(AZ::ComponentDescriptor::DependencyArrayType& incompatible)
    {
        incompatible.push_back(AZ_CRC_CE("Shell3DService"));
    }

    void Shell3DSystemComponent::GetRequiredServices([[maybe_unused]] AZ::ComponentDescriptor::DependencyArrayType& required)
    {
    }

    void Shell3DSystemComponent::GetDependentServices([[maybe_unused]] AZ::ComponentDescriptor::DependencyArrayType& dependent)
    {
    }

    Shell3DSystemComponent::Shell3DSystemComponent()
    {
        if (Shell3DInterface::Get() == nullptr)
        {
            Shell3DInterface::Register(this);
        }
    }

    Shell3DSystemComponent::~Shell3DSystemComponent()
    {
        if (Shell3DInterface::Get() == this)
        {
            Shell3DInterface::Unregister(this);
        }
    }

    void Shell3DSystemComponent::Init()
    {
    }

    void Shell3DSystemComponent::Activate()
    {
        Shell3DRequestBus::Handler::BusConnect();
    }

    void Shell3DSystemComponent::Deactivate()
    {
        Shell3DRequestBus::Handler::BusDisconnect();
    }
}
