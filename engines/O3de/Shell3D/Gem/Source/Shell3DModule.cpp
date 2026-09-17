
#include <AzCore/Memory/SystemAllocator.h>
#include <AzCore/Module/Module.h>

#include "Shell3DSystemComponent.h"

#include <Shell3D/Shell3DTypeIds.h>

namespace Shell3D
{
    class Shell3DModule
        : public AZ::Module
    {
    public:
        AZ_RTTI(Shell3DModule, Shell3DModuleTypeId, AZ::Module);
        AZ_CLASS_ALLOCATOR(Shell3DModule, AZ::SystemAllocator);

        Shell3DModule()
            : AZ::Module()
        {
            // Push results of [MyComponent]::CreateDescriptor() into m_descriptors here.
            m_descriptors.insert(m_descriptors.end(), {
                Shell3DSystemComponent::CreateDescriptor(),
            });
        }

        /**
         * Add required SystemComponents to the SystemEntity.
         */
        AZ::ComponentTypeList GetRequiredSystemComponents() const override
        {
            return AZ::ComponentTypeList{
                azrtti_typeid<Shell3DSystemComponent>(),
            };
        }
    };
}// namespace Shell3D

#if defined(O3DE_GEM_NAME)
AZ_DECLARE_MODULE_CLASS(AZ_JOIN(Gem_, O3DE_GEM_NAME), Shell3D::Shell3DModule)
#else
AZ_DECLARE_MODULE_CLASS(Gem_Shell3D, Shell3D::Shell3DModule)
#endif
