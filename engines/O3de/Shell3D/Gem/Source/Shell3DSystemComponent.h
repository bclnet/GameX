
#pragma once

#include <AzCore/Component/Component.h>

#include <Shell3D/Shell3DBus.h>

namespace Shell3D
{
    class Shell3DSystemComponent
        : public AZ::Component
        , protected Shell3DRequestBus::Handler
    {
    public:
        AZ_COMPONENT_DECL(Shell3DSystemComponent);

        static void Reflect(AZ::ReflectContext* context);

        static void GetProvidedServices(AZ::ComponentDescriptor::DependencyArrayType& provided);
        static void GetIncompatibleServices(AZ::ComponentDescriptor::DependencyArrayType& incompatible);
        static void GetRequiredServices(AZ::ComponentDescriptor::DependencyArrayType& required);
        static void GetDependentServices(AZ::ComponentDescriptor::DependencyArrayType& dependent);

        Shell3DSystemComponent();
        ~Shell3DSystemComponent();

    protected:
        ////////////////////////////////////////////////////////////////////////
        // Shell3DRequestBus interface implementation

        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // AZ::Component interface implementation
        void Init() override;
        void Activate() override;
        void Deactivate() override;
        ////////////////////////////////////////////////////////////////////////
    };
}
